using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.RabbitMQ
{
	public class Consumer : DefaultBasicConsumer
	{
		private readonly ILog c_logger;
		private readonly IMessageDeliveryHandler c_messageDeliveryHandler;


		public Consumer(
			IModel channel,
			ILog logger,
			IMessageDeliveryHandler messageDeliveryHandler)
			: base(channel)
		{
			this.c_logger = logger;
			this.c_messageDeliveryHandler = messageDeliveryHandler;

			this.c_logger.Info();
		}


		public override void HandleBasicDeliver(
			string consumerTag,
			ulong deliveryTag,
			bool redelivered,
			string exchange,
			string routingKey,
			IBasicProperties properties,
			byte[] body)
		{
			this.c_logger.DebugFormat("Handling message, consumer tag = {0}, delivery tag = {1}, type header = {2}", consumerTag, deliveryTag, properties.Type);

			this.c_messageDeliveryHandler.Handle(
				new SubscriptionMessage(
						body: body,
						consumerTag: consumerTag,
						deliveryTag: deliveryTag,
						exchange: exchange,
						redelivered: redelivered,
						routingKey: routingKey,
						appId: properties.AppId,
						clusterId: properties.ClusterId,
						contentEncoding: properties.ContentEncoding,
						contentType: properties.ContentType,
						correlationId: properties.CorrelationId,
						deliveryMode: properties.DeliveryMode,
						expiration: properties.Expiration,
						messageId: properties.MessageId,
						priority: properties.Priority,
						replyTo: properties.ReplyTo,
						type: properties.Type,
						userId: properties.UserId));

			this.c_logger.DebugFormat("Handled message, consumer tag = {0}, delivery tag = {1}, type header = {2}", consumerTag, deliveryTag, properties.Type);
		}
	}
}