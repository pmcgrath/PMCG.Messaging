using Common.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Publisher
	{
		private readonly ILog c_logger;
		private readonly TimeSpan c_timeout;
		private readonly CancellationToken c_cancellationToken;
		private readonly ThreadLocal<IModel> c_threadLocalChannel;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> c_unconfirmedPublicationResults;


		private IModel Channel { get { return this.c_threadLocalChannel.Value; } }


		public Publisher(
			IConnection connection,
			TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

			this.c_timeout = timeout;
			this.c_cancellationToken = cancellationToken;

			this.c_threadLocalChannel = new ThreadLocal<IModel>(() =>
				{
					this.c_logger.Info("ThreadLocalChannel About to create channel");
					var _channel = connection.CreateModel();
					_channel.ConfirmSelect();
					_channel.ModelShutdown += this.OnChannelShutdown;
					_channel.BasicAcks += this.OnChannelAcked;
					_channel.BasicNacks += this.OnChannelNacked;
					this.c_logger.Info("ThreadLocalChannel Channel created");

					return _channel;
				});

			this.c_unconfirmedPublicationResults = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

			this.c_logger.Info("ctor Completed");
		}


		public void Publish(
			QueuedMessage message)
		{
			this.c_logger.DebugFormat("Publish About to publish message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation already requested");
			Check.Ensure(this.Channel.IsOpen, "Channel is not open");

			var _properties = this.CreateMessageProperties(message);
			var _messageBody = this.CreateMessageBody(message);

			this.Channel.BasicPublish(
				message.ExchangeName,
				message.RoutingKey,
				_properties,
				_messageBody);

			var _timedOut = false;
			var _confirmed = this.Channel.WaitForConfirms(this.c_timeout, out _timedOut);
			if (_timedOut)
			{
				throw new ApplicationException("Timed out waiting for publication confirmation");
			}
			if (!_confirmed)
			{
				throw new ApplicationException("Not confirmed for publication confirmation");
			}

			this.c_logger.DebugFormat("Publish Completed publishing message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
		}


		public Task<bool> PublishAsync(
			QueuedMessage message)
		{
			this.c_logger.DebugFormat("PublishAsync About to publish message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation already requested");
			Check.Ensure(this.Channel.IsOpen, "Channel is not open");

			var _properties = this.CreateMessageProperties(message);
			var _messageBody = this.CreateMessageBody(message);

			var _channelIdentifier = this.Channel.ToString();
			var _deliveryTag = this.Channel.NextPublishSeqNo;
			var _unconfirmedPublicationResultKey = string.Format("{0}::{1}", _channelIdentifier, _deliveryTag);
			var _result = new TaskCompletionSource<bool>();
			try
			{
				this.c_unconfirmedPublicationResults.TryAdd(_unconfirmedPublicationResultKey, _result);
				this.Channel.BasicPublish(
					message.ExchangeName,
					message.RoutingKey,
					_properties,
					_messageBody);
			}
			catch
			{
				this.c_unconfirmedPublicationResults.TryRemove(_unconfirmedPublicationResultKey, out _result);
				throw;
			}

			this.c_logger.DebugFormat("PublishAsync Completed publishing message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
			return _result.Task;
		}


		private void OnChannelShutdown(
			IModel channel,
			ShutdownEventArgs reason)
		{
			this.c_logger.WarnFormat("OnChannelShuutdown Starting, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);

			var _channelIdentifier = channel.ToString();
			var _highestDeliveryTag = this.c_unconfirmedPublicationResults
				.Select(item => item.Key)
				.Where(key => key.StartsWith(_channelIdentifier))
				.Select(key => ulong.Parse(key.Substring(key.IndexOf("::") + "::".Length)))
				.OrderByDescending(deliveryTag => deliveryTag)
				.FirstOrDefault();
			if (_highestDeliveryTag > 0)
			{
				var _exception = new ApplicationException("Channel was closed");
				this.ProcessDeliveryTags(_channelIdentifier, true, _highestDeliveryTag, publicationResult => publicationResult.SetException(_exception));
			}

			this.c_logger.WarnFormat("OnChannelShuutdown Completed, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
		}


		private void OnChannelAcked(
			IModel channel,
			BasicAckEventArgs args)
		{
			this.c_logger.DebugFormat("OnChannelAcked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			this.ProcessDeliveryTags(channel.ToString(), args.Multiple, args.DeliveryTag, publicationResult => publicationResult.SetResult(true));

			this.c_logger.DebugFormat("OnChannelAcked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void OnChannelNacked(
			IModel channel,
			BasicNackEventArgs args)
		{
			this.c_logger.DebugFormat("OnChannelNacked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			var _exception = new ApplicationException("Publish was nacked by the broker");
			this.ProcessDeliveryTags(channel.ToString(), args.Multiple, args.DeliveryTag, publicationResult => publicationResult.SetException(_exception));

			this.c_logger.DebugFormat("OnChannelNacked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void ProcessDeliveryTags(
			string channelIdentifier,
			bool isMultiple,
			ulong highestDeliveryTag,
			Action<TaskCompletionSource<bool>> action)
		{
			var _deliveryTags = new[] { highestDeliveryTag };
			if (isMultiple)
			{
				_deliveryTags = this.c_unconfirmedPublicationResults
					.Select(item => item.Key)
					.Where(key => key.StartsWith(channelIdentifier))
					.Select(key => ulong.Parse(key.Substring(key.IndexOf("::") + "::".Length)))
					.Where(deliveryTag => deliveryTag <= highestDeliveryTag)
					.ToArray();
			}

			foreach (var _deliveryTag in _deliveryTags)
			{
				var _unconfirmedPublicationResultKey = string.Format("{0}::{1}", channelIdentifier, _deliveryTag);
				if (!this.c_unconfirmedPublicationResults.ContainsKey(_unconfirmedPublicationResultKey)) { continue; }

				TaskCompletionSource<bool> _publicationResult = null;
				this.c_unconfirmedPublicationResults.TryRemove(_unconfirmedPublicationResultKey, out _publicationResult);
				action(_publicationResult);
			}
		}


		private IBasicProperties CreateMessageProperties(
			QueuedMessage message)
		{
			var _result = this.Channel.CreateBasicProperties();
			_result.ContentType = "application/json";
			_result.DeliveryMode = message.DeliveryMode;
			_result.Type = message.TypeHeader;
			_result.MessageId = message.Data.Id.ToString();
			_result.CorrelationId = message.Data.CorrelationId;

			return _result;
		}


		private byte[] CreateMessageBody(
			QueuedMessage message)
		{
			var _messageJson = JsonConvert.SerializeObject(message.Data);
			var _result = Encoding.UTF8.GetBytes(_messageJson);

			return _result;
		}
	}
}