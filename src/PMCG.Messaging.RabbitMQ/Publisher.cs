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

			try
			{
				foreach (var _queuedMessage in this.c_queuedMessages.GetConsumingEnumerable(this.c_cancellationToken))
				{
					try
					{
						this.c_logger.DebugFormat("About to publish message with Id ({0})", _queuedMessage.Data.Id);
						this.Publish(_queuedMessage);
					}
					catch (Exception genericException)
					{
						this.c_logger.ErrorFormat("Publication error {0}", genericException.Message);
						this.c_queuedMessages.Add(_queuedMessage);
						throw;
					}
				}
			}
			catch (OperationCanceledException)
			{
				this.c_logger.Info("Operation canceled");
			}
			catch (Exception genericException)
			{
				this.c_logger.ErrorFormat("EXCEPTION {0}", genericException.Message);
			}

			if (this.c_channel.IsOpen)
			{
				// Cater for race condition, when stopping - Is open but when we get to this line it is closed
				try { this.c_channel.Close(); } catch { }
			}

			this.c_logger.Info("Completed");
		}


		private void Publish(
			QueuedMessage subject)
		{
			this.c_logger.DebugFormat("About to publish message with Id {0} to exchange {1}", subject.Data.Id, subject.ExchangeName);

			var _basicProperties = this.c_channel.CreateBasicProperties();
			_basicProperties.ContentType = "application/json";
			_basicProperties.DeliveryMode = subject.DeliveryMode;
			_basicProperties.Type = subject.TypeHeader;

			var _messageJson = JsonConvert.SerializeObject(subject.Data);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			this.c_channel.BasicPublish(
				subject.ExchangeName,
				subject.RoutingKey,
				_basicProperties,
				_messageBody);

			this.c_logger.DebugFormat("Completed publish message with Id {0} to exchange {1}", subject.Data.Id, subject.ExchangeName);
		}
	}
}