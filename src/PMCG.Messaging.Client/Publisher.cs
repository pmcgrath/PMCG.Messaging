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
		private readonly BlockingCollection<TaskCompletionSource<PublisherResult>> c_queuedPublicationTasks;
		private readonly CancellationToken c_cancellationToken;
		private readonly IModel c_channel;
		private readonly ConcurrentDictionary<ulong, TaskCompletionSource<PublisherResult>> c_unconfirmedPublisherResults;


		private bool c_hasBeenStarted;
		private bool c_isCompleted;


		public Publisher(
			IConnection connection,
			BlockingCollection<TaskCompletionSource<PublisherResult>> queuedPublicationTasks,
			CancellationToken cancellationToken)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

			this.c_queuedPublicationTasks = queuedPublicationTasks;
			this.c_cancellationToken = cancellationToken;

			this.c_channel = connection.CreateModel();
			this.c_channel.ConfirmSelect();
			this.c_channel.ModelShutdown += this.OnChannelShutdown;
			this.c_channel.BasicAcks += this.OnChannelAcked;
			this.c_channel.BasicNacks += this.OnChannelNacked;

			this.c_unconfirmedPublisherResults = new ConcurrentDictionary<ulong, TaskCompletionSource<PublisherResult>>();

			this.c_logger.Info("ctor Completed");
		}


		public void Start()
		{
			this.c_logger.Info("Start Starting");
			Check.Ensure(!this.c_hasBeenStarted, "Publisher has already been started, can only do so once");
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation token is already canceled");

			try
			{
				this.c_hasBeenStarted = true;
				this.RunPublicationLoop();
			}
			catch (Exception exception)
			{
				this.c_logger.ErrorFormat("Exception : {0}", exception);
				throw;
			}
			finally
			{
				if (this.c_channel.IsOpen)
				{
					// Cater for race condition, when stopping - Is open but when we get to this line it is closed
					try { this.c_channel.Close(); } catch { }
				}

				this.c_isCompleted = true;
				this.c_logger.Info("Start Completed publishing");
			}
		}


		private void RunPublicationLoop()
		{
			try
			{
				foreach (var _queuedPublicationTask in this.c_queuedPublicationTasks.GetConsumingEnumerable(this.c_cancellationToken))
				{
					try
					{
						this.Publish(_queuedPublicationTask);
					}
					catch (OperationCanceledException)
					{
						this.c_logger.Warn("RunPublicationLoop Operation canceled");
						this.c_queuedPublicationTasks.Add(_queuedPublicationTask);
					}
					catch
					{
						this.c_queuedPublicationTasks.Add(_queuedPublicationTask);
						throw;
					}
				}
			}
			catch (OperationCanceledException)
			{
				this.c_logger.Warn("RunPublicationLoop Operation canceled");
			}
		}


		public void Publish(
			TaskCompletionSource<PublisherResult> publicationTask)
		{
			// pmcg look at this, order !!!!

			var _message = (QueuedMessage)publicationTask.Task.AsyncState;

			this.c_logger.DebugFormat("Publish About to publish message with Id {0} to exchange {1}", _message.Data.Id, _message.ExchangeName);
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation already requested");
			Check.Ensure(this.c_channel.IsOpen, "Channel is not open");

			var _properties = this.c_channel.CreateBasicProperties();
			_properties.ContentType = "application/json";
			_properties.DeliveryMode = _message.DeliveryMode;
			_properties.Type = _message.TypeHeader;
			_properties.MessageId = _message.Id.ToString();
			// Only set if null, otherwise library will blow up, default is string.Empty, if set to null will blow up in library
			if (_message.Data.CorrelationId != null) { _properties.CorrelationId = _message.Data.CorrelationId; }

			var _messageJson = JsonConvert.SerializeObject(_message.Data);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			var _deliveryTag = this.c_channel.NextPublishSeqNo;
			try
			{
				this.c_unconfirmedPublisherResults.TryAdd(_deliveryTag, publicationTask);
				this.c_channel.BasicPublish(
					_message.ExchangeName,
					_message.RoutingKey,
					_properties,
					_messageBody);
			}
			catch
			{
				this.c_unconfirmedPublisherResults.TryRemove(_deliveryTag, out publicationTask);
				throw;
			}

			this.c_logger.DebugFormat("Publish Completed publishing message with Id {0} to exchange {1}", _message.Data.Id, _message.ExchangeName);
		}


		private void OnChannelShutdown(
			IModel channel,
			ShutdownEventArgs reason)
		{
			this.c_logger.WarnFormat("OnChannelShuutdown Starting, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);

			
			var _highestDeliveryTag = this.c_unconfirmedPublisherResults
				.Select(item => item.Key)
				.OrderByDescending(deliveryTag => deliveryTag)
				.FirstOrDefault();
			if (_highestDeliveryTag > 0)
			{
				var _context = string.Format("Code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
				this.ProcessDeliveryTags(
					true,
					_highestDeliveryTag,
					publisherResult => publisherResult.SetResult(
						new PublisherResult(
							(QueuedMessage)publisherResult.Task.AsyncState,
							PublisherResultStatus.ChannelShutdown,
							_context)));
			}

			this.c_logger.WarnFormat("OnChannelShuutdown Completed, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
		}


		private void OnChannelAcked(
			IModel channel,
			BasicAckEventArgs args)
		{
			this.c_logger.DebugFormat("OnChannelAcked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			this.ProcessDeliveryTags(
				args.Multiple,
				args.DeliveryTag,
				publisherResult => publisherResult.SetResult(
					new PublisherResult(
						(QueuedMessage)publisherResult.Task.AsyncState,
						PublisherResultStatus.Acked)));

			this.c_logger.DebugFormat("OnChannelAcked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void OnChannelNacked(
			IModel channel,
			BasicNackEventArgs args)
		{
			this.c_logger.DebugFormat("OnChannelNacked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			this.ProcessDeliveryTags(
				args.Multiple,
				args.DeliveryTag,
				publisherResult => publisherResult.SetResult(
					new PublisherResult(
						(QueuedMessage)publisherResult.Task.AsyncState,
						PublisherResultStatus.Nacked)));

			this.c_logger.DebugFormat("OnChannelNacked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void ProcessDeliveryTags(
			bool isMultiple,
			ulong highestDeliveryTag,
			Action<TaskCompletionSource<PublisherResult>> action)
		{
			// Critical section - What if an ack followed by a nack and the two trying to do work at the same time
			var _deliveryTags = new[] { highestDeliveryTag };
			if (isMultiple)
			{
				_deliveryTags = this.c_unconfirmedPublisherResults
					.Select(item => item.Key)
					.Where(deliveryTag => deliveryTag <= highestDeliveryTag)
					.ToArray();
			}

			foreach (var _deliveryTag in _deliveryTags)
			{
				if (!this.c_unconfirmedPublisherResults.ContainsKey(_deliveryTag)) { continue; }

				TaskCompletionSource<PublisherResult> _publisherResult = null;
				this.c_unconfirmedPublisherResults.TryRemove(_deliveryTag, out _publisherResult);
				action(_publisherResult);
			}
		}
	}
}