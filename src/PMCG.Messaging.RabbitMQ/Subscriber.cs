using Newtonsoft.Json;
using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;
using System.Text;


namespace PMCG.Messaging.RabbitMQ
{
	public class Subscriber : IMessageDeliveryHandler
	{
		private readonly ILog c_logger;
		private readonly MessageSubscriptions c_messageSubscriptionConfigurations;
		private readonly IModel c_channel;
		private readonly Consumer c_consumer;


		public Subscriber(
			ILog logger,
			IConnection connection,
			MessageSubscriptions messageSubscriptionConfigurations)
		{
			this.c_logger = logger;
			this.c_messageSubscriptionConfigurations = messageSubscriptionConfigurations;

			this.c_logger.Info("About to create channel");
			this.c_channel = connection.CreateModel();

			this.c_logger.Info("About to create consumer");
			this.c_consumer = new Consumer(this.c_channel, this.c_logger, this);

			this.c_logger.Info("Completed");
		}


		public void Start()
		{
			this.c_logger.Info();

			foreach (var _queueName in this.c_messageSubscriptionConfigurations.GetDistinctQueueNames())
			{
				this.c_logger.InfoFormat("Consume for queue {0}", _queueName);
				this.c_channel.BasicConsume(_queueName, false, this.c_consumer);
			}

			this.c_logger.Info("Completed");
		}


		public void Stop()
		{
			this.c_logger.Info();
			// Cater for case where connection has been broken
			try { this.c_channel.Close(); } catch { }
			this.c_logger.Info("Completed");
		}


		public void Handle(
			SubscriptionMessage subject)
		{
			var _logMessageContext = string.Format("type header = {0} and delivery tag = {1}", subject.Type, subject.DeliveryTag);
			this.c_logger.DebugFormat("About to handle message, {0}", _logMessageContext);

			if (!this.c_messageSubscriptionConfigurations.HasConfiguration(subject.Type))
			{
				this.c_logger.DebugFormat("No match found for message, {0}", _logMessageContext);
				this.c_channel.BasicNack(subject.DeliveryTag, false, false);
				return;
			}

			MessageSubscriptionActionResult _actionResult = MessageSubscriptionActionResult.None;
			try
			{
				this.c_logger.DebugFormat("About to deserialze message for message, {0}", _logMessageContext);
				var _configuration = this.c_messageSubscriptionConfigurations[subject.Type];
				var _messageJson = Encoding.UTF8.GetString(subject.Body);
				var _message = JsonConvert.DeserializeObject(_messageJson, _configuration.Type);
				
				this.c_logger.DebugFormat("About to invoke action for message, {0}", _logMessageContext);
				_actionResult = _configuration.Action(_message as Message);
			}
			catch
			{
				_actionResult = MessageSubscriptionActionResult.Errored;
			}
			this.c_logger.DebugFormat("Completed processing for message, {0}, result is {1}", _logMessageContext, _actionResult);


			if (_actionResult == MessageSubscriptionActionResult.Errored)
			{
				this.c_channel.BasicNack(subject.DeliveryTag, false, false);
			}
			else if (_actionResult == MessageSubscriptionActionResult.Completed)
			{
				this.c_channel.BasicAck(subject.DeliveryTag, false);
			}
			else if (_actionResult == MessageSubscriptionActionResult.Requeue)
			{
				this.c_channel.BasicReject(subject.DeliveryTag, true);
			}

			this.c_logger.DebugFormat("Completed handling message, {0}, Result is {1}", _logMessageContext, _actionResult);
		}
	}
}