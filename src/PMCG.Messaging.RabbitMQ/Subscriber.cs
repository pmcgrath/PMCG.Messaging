using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Threading;


namespace PMCG.Messaging.RabbitMQ
{
	public class Subscriber
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;
		private readonly IModel c_channel;
		private readonly CancellationToken c_cancellationToken;
		private readonly SubscriptionMessageProcessor c_messageProcessor;


		private bool c_hasBeenStarted;


		public Subscriber(
			ILog logger,
			IConnection connection,
			BusConfiguration configuration,
			CancellationToken cancellationToken)
		{
			this.c_logger = logger;
			this.c_configuration = configuration;
			this.c_cancellationToken = cancellationToken;

			this.c_logger.Info("About to create channel");
			this.c_channel = connection.CreateModel();
			this.c_channel.BasicQos(0, this.c_configuration.SubscriptionMessagePrefetchCount, false);

			this.c_logger.Info("About to create subscription message processor");
			this.c_messageProcessor = new SubscriptionMessageProcessor(this.c_logger, this.c_configuration);

			this.c_logger.Info("Completed");
		}


		public void Start()
		{
			this.c_logger.Info();

			Check.Ensure(!this.c_hasBeenStarted, "Subsriber has already been started, can only do so once");
			this.c_hasBeenStarted = true;

			var _consumer = new QueueingBasicConsumer(this.c_channel);
			foreach (var _queueName in this.c_configuration.MessageSubscriptions.GetDistinctQueueNames())
			{
				this.c_logger.InfoFormat("Consume for queue {0}", _queueName);
				var _consumerTag = this.c_channel.BasicConsume(_queueName, false, _consumer);
				this.c_logger.InfoFormat("Consume for queue {0}, consumer tag is {1}", _queueName, _consumerTag);
			}

			this.c_logger.Info("About to start consuming loop");
			var _timeoutInMilliseconds = (int)this.c_configuration.SubscriptionDequeueTimeout.TotalMilliseconds;
			while (!this.c_cancellationToken.IsCancellationRequested)
			{
				try
				{
					object _result = null;
					if (_consumer.Queue.Dequeue(_timeoutInMilliseconds, out _result))
					{
						this.c_messageProcessor.Process(this.c_channel, (BasicDeliverEventArgs)_result);
					}
				}
				catch (EndOfStreamException)
				{
					this.c_logger.Info("End of stream");
					break;
				}
				catch (Exception genericException)
				{
					this.c_logger.ErrorFormat("Exception : {0}", genericException);
					throw;
				}
			}

			if (this.c_channel.IsOpen)
			{
				// Cater for race condition, when stopping - Is open but when we get to this line it is closed
				try { this.c_channel.Close(); } catch { }
			}

			this.c_logger.Info("Completed consuming");
		}
	}
}