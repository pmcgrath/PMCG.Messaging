using Newtonsoft.Json;
using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;


namespace PMCG.Messaging.RabbitMQ
{
	public class Publisher
	{
		private readonly ILog c_logger;
		private readonly IModel c_channel;
		private readonly CancellationToken c_cancellationToken;
		private readonly BlockingCollection<QueuedMessage> c_queuedMessages;


		private bool c_hasBeenStarted;
		private bool c_isCompleted;


		public bool IsCompleted { get { return this.c_isCompleted; } }


		public Publisher(
			ILog logger,
			IConnection connection,
			CancellationToken cancellationToken,
			BlockingCollection<QueuedMessage> queuedMessages)
		{
			this.c_logger = logger;
			this.c_cancellationToken = cancellationToken;
			this.c_queuedMessages = queuedMessages;

			this.c_logger.Info("About to create channel");
			this.c_channel = connection.CreateModel();
			this.c_logger.Info("Completed");
		}


		public void Start()
		{
			this.c_logger.Info();
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
				this.c_logger.Info("Completed publishing");
			}
		}


		private void RunPublicationLoop()
		{
			try
			{
				foreach (var _queuedMessage in this.c_queuedMessages.GetConsumingEnumerable(this.c_cancellationToken))
				{
					try
					{
						this.c_logger.DebugFormat("About to publish message with Id ({0})", _queuedMessage.Data.Id);
						this.Publish(_queuedMessage);
					}
					catch (Exception exception)
					{
						this.c_logger.ErrorFormat("Publication error for message with Id ({0}) error message: {0}, requeuing the messaage",
							_queuedMessage.Data.Id,
							exception.Message);
						this.c_queuedMessages.Add(_queuedMessage);
						throw;
					}
				}
			}
			catch (OperationCanceledException)
			{
				this.c_logger.Info("Operation canceled");
			}
		}


		private void Publish(
			QueuedMessage message)
		{
			this.c_logger.DebugFormat("About to publish message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);

			var _properties = this.c_channel.CreateBasicProperties();
			_properties.ContentType = "application/json";
			_properties.DeliveryMode = message.DeliveryMode;
			_properties.Type = message.TypeHeader;
			_properties.MessageId = message.Data.Id.ToString();
//			_properties.UserId = Environment.UserName;

			var _messageJson = JsonConvert.SerializeObject(message.Data);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			this.c_channel.BasicPublish(
				message.ExchangeName,
				message.RoutingKey,
				_properties,
				_messageBody);

			this.c_logger.DebugFormat("Completed publish message with Id {0} to exchange {1}", message.Data.Id, message.ExchangeName);
		}
	}
}