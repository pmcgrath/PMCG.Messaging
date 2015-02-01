using log4net;
using Newtonsoft.Json;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace PMCG.Messaging.Client
{
	public class ConsumerMessageProcessor
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;


		public ConsumerMessageProcessor(
			BusConfiguration configuration)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");
			
			this.c_configuration = configuration;
			this.c_logger.Info("ctor Completed");
		}


		public void Process(
			IModel channel,
			BasicDeliverEventArgs message)
		{
			var _logMessageContext = string.Format("type header = {0}, message Id = {1}, correlation Id = {2} and delivery tag = {3}",
				message.BasicProperties.Type,
				message.BasicProperties.MessageId,
				message.BasicProperties.CorrelationId,
				message.DeliveryTag);
			this.c_logger.DebugFormat("Process About to handle message, {0}", _logMessageContext);

			if (!this.c_configuration.MessageConsumers.HasConfiguration(message.BasicProperties.Type))
			{
				this.c_logger.DebugFormat("Process No match found for message, {0}", _logMessageContext);
				//pending - see errored below
				channel.BasicNack(message.DeliveryTag, false, false);
				return;
			}

			var _configuration = this.c_configuration.MessageConsumers[message.BasicProperties.Type];
			var _actionResult = ConsumerHandlerResult.None;
			try
			{
				this.c_logger.DebugFormat("Process About to deserialze message for message, {0}", _logMessageContext);
				var _messageJson = Encoding.UTF8.GetString(message.Body);
				var _message = JsonConvert.DeserializeObject(_messageJson, _configuration.Type);

				this.c_logger.DebugFormat("Process About to invoke action for message, {0}", _logMessageContext);
				_actionResult = _configuration.Action(_message as Message);
			}
			catch (Exception exception)
			{
				this.c_logger.WarnFormat("Process Encountered error for message {0} Error: {1}", _logMessageContext, exception.InstrumentationString());
				_actionResult = ConsumerHandlerResult.Errored;
			}
			this.c_logger.DebugFormat("Process Completed processing for message, {0}, result is {1}", _logMessageContext, _actionResult);

			if (_actionResult == ConsumerHandlerResult.Errored)
			{
				//pending - should i use configuration properties
				// Nack, do not requeue, dead letter the message if dead letter exchange configured for the queue
				channel.BasicNack(message.DeliveryTag, false, false);
			}
			else if (_actionResult == ConsumerHandlerResult.Completed)
			{
				channel.BasicAck(message.DeliveryTag, false);
			}
			else if (_actionResult == ConsumerHandlerResult.Requeue)
			{
				//pending does this make sense ?
				channel.BasicReject(message.DeliveryTag, true);
			}

			this.c_logger.DebugFormat("Process Completed handling message, {0}, Result is {1}", _logMessageContext, _actionResult);
		}
	}
}
