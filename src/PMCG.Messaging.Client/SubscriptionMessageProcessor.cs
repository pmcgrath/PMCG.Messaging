using Newtonsoft.Json;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace PMCG.Messaging.Client
{
	public class SubscriptionMessageProcessor
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;


		public SubscriptionMessageProcessor(
			ILog logger,
			BusConfiguration configuration)
		{
			this.c_logger = logger;
			this.c_configuration = configuration;
			this.c_logger.Info("Completed");
		}


		public void Process(
			IModel channel,
			BasicDeliverEventArgs message)
		{
			var _logMessageContext = string.Format("type header = {0}, message Id = {1} and delivery tag = {2}", 
				message.BasicProperties.Type,
				message.BasicProperties.MessageId,
				message.DeliveryTag);
			this.c_logger.DebugFormat("About to handle message, {0}", _logMessageContext);

			if (!this.c_configuration.MessageSubscriptions.HasConfiguration(message.BasicProperties.Type))
			{
				this.c_logger.DebugFormat("No match found for message, {0}", _logMessageContext);
				//pending - see errored below
				channel.BasicNack(message.DeliveryTag, false, false);
				return;
			}

			var _configuration = this.c_configuration.MessageSubscriptions[message.BasicProperties.Type];
			var _actionResult = MessageSubscriptionActionResult.None;
			try
			{
				this.c_logger.DebugFormat("About to deserialze message for message, {0}", _logMessageContext);
				var _messageJson = Encoding.UTF8.GetString(message.Body);
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
				//pending - should i use configuration properties
				// Nack, do not requeue, dead letter the message if dead letter exchange configured for the queue
				channel.BasicNack(message.DeliveryTag, false, false);
			}
			else if (_actionResult == MessageSubscriptionActionResult.Completed)
			{
				channel.BasicAck(message.DeliveryTag, false);
			}
			else if (_actionResult == MessageSubscriptionActionResult.Requeue)
			{
				//pending does this make sense ?
				channel.BasicReject(message.DeliveryTag, true);
			}

			this.c_logger.DebugFormat("Completed handling message, {0}, Result is {1}", _logMessageContext, _actionResult);
		}
	}
}
