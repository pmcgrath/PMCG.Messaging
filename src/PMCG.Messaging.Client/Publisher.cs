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
		private readonly BlockingCollection<Publication> c_publicationQueue;
		private readonly CancellationToken c_cancellationToken;
		private readonly IModel c_channel;
		private readonly ConcurrentDictionary<ulong, Publication> c_unconfirmedPublications;


		private bool c_hasBeenStarted;
		private bool c_isCompleted;


		public Publisher(
			IConnection connection,
			BlockingCollection<Publication> publicationQueue,
			CancellationToken cancellationToken)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

			this.c_publicationQueue = publicationQueue;
			this.c_cancellationToken = cancellationToken;

			this.c_channel = connection.CreateModel();
			this.c_channel.ConfirmSelect();
			this.c_channel.ModelShutdown += this.OnChannelShutdown;
			this.c_channel.BasicAcks += this.OnChannelAcked;
			this.c_channel.BasicNacks += this.OnChannelNacked;

			this.c_unconfirmedPublications = new ConcurrentDictionary<ulong, Publication>();

			this.c_logger.Info("ctor Completed");
		}


		public Task Start()
		{
			this.c_logger.Info("Start Starting");
			Check.Ensure(!this.c_hasBeenStarted, "Publisher has already been started, can only do so once");
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation token is already canceled");

			var _result = new Task(
				() =>
					{
						try
						{
							this.c_hasBeenStarted = true;
							this.RunPublicationLoop();
						}
						catch (Exception exception)
						{
							this.c_logger.ErrorFormat("Exception : {0}", exception.InstrumentationString());
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
							this.c_logger.Info("Start Publisher task completed");
						}
					},
				this.c_cancellationToken,
				TaskCreationOptions.LongRunning);
			_result.Start();

			this.c_logger.Info("Start Completed publishing");
			return _result;
		}


		private void RunPublicationLoop()
		{
			// OperationCanceledException exception is thrown when the cancellation token is cancelled when using the consuming enumerable
			try
			{
				foreach (var _publication in this.c_publicationQueue.GetConsumingEnumerable(this.c_cancellationToken))
				{
					try
					{
						this.Publish(_publication);
					}
					catch (OperationCanceledException)
					{
						this.c_publicationQueue.Add(_publication);
						this.c_logger.Warn("RunPublicationLoop Operation canceled");
					}
					catch
					{
						this.c_publicationQueue.Add(_publication);
						throw;
					}
				}
			}
			catch (OperationCanceledException)
			{
				this.c_logger.Warn("RunPublicationLoop Operation canceled");
			}
		}


		private void Publish(
			Publication publication)
		{
			this.c_logger.DebugFormat("Publish About to publish message with Id {0} to exchange {1}", publication.Id, publication.ExchangeName);

			var _properties = this.c_channel.CreateBasicProperties();
			_properties.ContentType = "application/json";
			_properties.DeliveryMode = publication.DeliveryMode;
			_properties.Type = publication.TypeHeader;
			_properties.MessageId = publication.Id;
			// Only set if null, otherwise library will blow up, default is string.Empty, if set to null will blow up in library
			if (publication.CorrelationId != null) { _properties.CorrelationId = publication.CorrelationId; }

			var _messageJson = JsonConvert.SerializeObject(publication.Message);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			var _deliveryTag = this.c_channel.NextPublishSeqNo;
			try
			{
				this.c_unconfirmedPublications.TryAdd(_deliveryTag, publication);
				this.c_channel.BasicPublish(
					publication.ExchangeName,
					publication.RoutingKey,
					_properties,
					_messageBody);
			}
			catch
			{
				this.c_unconfirmedPublications.TryRemove(_deliveryTag, out publication);
				throw;
			}

			this.c_logger.DebugFormat("Publish Completed publishing message with Id {0} to exchange {1}", publication.Id, publication.ExchangeName);
		}


		private void OnChannelShutdown(
			IModel channel,
			ShutdownEventArgs reason)
		{
			this.c_logger.WarnFormat("OnChannelShuutdown Starting, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);

			var _highestDeliveryTag = this.c_unconfirmedPublications
				.Keys
				.OrderByDescending(deliveryTag => deliveryTag)
				.FirstOrDefault();
			if (_highestDeliveryTag > 0)
			{
				var _context = string.Format("Code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
				this.ProcessDeliveryTags(
					true,
					_highestDeliveryTag,
					publication => publication.SetResult(PublicationResultStatus.ChannelShutdown, _context));
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
				publication => publication.SetResult(PublicationResultStatus.Acked));

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
				publication => publication.SetResult(PublicationResultStatus.Nacked));

			this.c_logger.DebugFormat("OnChannelNacked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void ProcessDeliveryTags(
			bool isMultiple,
			ulong highestDeliveryTag,
			Action<Publication> action)
		{
			// Critical section - What if an ack followed by a nack and the two trying to do work at the same time
			var _deliveryTags = new[] { highestDeliveryTag };
			if (isMultiple)
			{
				_deliveryTags = this.c_unconfirmedPublications
					.Keys
					.Where(deliveryTag => deliveryTag <= highestDeliveryTag)
					.ToArray();
			}

			Publication _publication = null;
			foreach (var _deliveryTag in _deliveryTags)
			{
				if (!this.c_unconfirmedPublications.ContainsKey(_deliveryTag)) { continue; }

				this.c_unconfirmedPublications.TryRemove(_deliveryTag, out _publication);
				action(_publication);
			}
		}
	}
}