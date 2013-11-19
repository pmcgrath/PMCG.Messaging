﻿using Common.Logging;
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
		private readonly CancellationToken c_cancellationToken;
		private readonly ThreadLocal<IModel> c_threadLocalChannel;
		private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> c_unconfirmedPublicationResults;


		private IModel Channel { get { return this.c_threadLocalChannel.Value; } }


		public Publisher(
			IConnection connection,
			CancellationToken cancellationToken)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

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


		public Task PublishAsync(
			QueuedMessage message)
		{
			this.c_logger.DebugFormat("PublishAsync About to publish message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation already requested");
			Check.Ensure(this.Channel.IsOpen, "Channel is not open");

			var _properties = this.Channel.CreateBasicProperties();
			_properties.ContentType = "application/json";
			_properties.DeliveryMode = message.DeliveryMode;
			_properties.Type = message.TypeHeader;
			_properties.MessageId = message.Id.ToString();
			// Only set if null, otherwise library will blow up, default is string.Empty, if set to null will blow up in library
			if (message.Data.CorrelationId != null) { _properties.CorrelationId = message.Data.CorrelationId; }

			var _messageJson = JsonConvert.SerializeObject(message.Data);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			var _channelIdentifier = this.Channel.ToString();
			var _deliveryTag = this.Channel.NextPublishSeqNo;
			var _unconfirmedPublicationResultKey = string.Format("{0}::{1}", _channelIdentifier, _deliveryTag);
			var _result = new TaskCompletionSource<bool>(message);
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
				this.ProcessDeliveryTags(_channelIdentifier, true, _highestDeliveryTag, publicationResult =>
					{
						var _queuedMessage = (QueuedMessage)publicationResult.Task.AsyncState;
						var _exceptionMessage = string.Format("Channel was closed, code is {0} and text is {1}, message with Id {0} is not acked by the broker", reason.ReplyCode, reason.ReplyText, _queuedMessage.Id);
						var _exception = new ApplicationException(_exceptionMessage);
						publicationResult.SetException(_exception);
					});
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

			this.ProcessDeliveryTags(channel.ToString(), args.Multiple, args.DeliveryTag, publicationResult =>
				{
					var _queuedMessage = (QueuedMessage)publicationResult.Task.AsyncState;
					var _exceptionMessage = string.Format("Publish for message with Id {0} was nacked by the broker", _queuedMessage.Id);
					var _exception = new ApplicationException(_exceptionMessage);
					publicationResult.SetException(_exception);
				});

			this.c_logger.DebugFormat("OnChannelNacked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void ProcessDeliveryTags(
			string channelIdentifier,
			bool isMultiple,
			ulong highestDeliveryTag,
			Action<TaskCompletionSource<bool>> action)
		{
			// Critical section - What if an ack followed by a nack and the two trying to do work at the same time
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
	}
}