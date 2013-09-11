using System;


namespace PMCG.Messaging.RabbitMQ
{
	public class SubscriptionMessage
	{
		// Message properties
		public readonly byte[] Body;
		public readonly string ConsumerTag;
		public readonly ulong DeliveryTag;
		public readonly string Exchange;
		public readonly bool Redelivered;
		public readonly string RoutingKey;
		// Header properties
		public readonly string AppId;
		public readonly string ClusterId;
		public readonly string ContentEncoding;
		public readonly string ContentType;
		public readonly string CorrelationId;
		public readonly byte DeliveryMode;
		public readonly string Expiration;
		public readonly string MessageId;
		public readonly byte Priority;
		public readonly string ReplyTo;
		public readonly string Type;
		public readonly string UserId;


		public SubscriptionMessage(
			byte[] body,
			string consumerTag,
			ulong deliveryTag,
			string exchange,
			bool redelivered,
			string routingKey,
			string appId = null,
			string clusterId = null,
			string contentEncoding = null,
			string contentType = null,
			string correlationId = null,
			byte deliveryMode = 0,
			string expiration = null,
			string messageId = null,
			byte priority = 0,
			string replyTo = null,
			string type = null,
			string userId = null)
		{
			this.Body = body;
			this.ConsumerTag = consumerTag;
			this.DeliveryTag = deliveryTag;
			this.Exchange = exchange;
			this.Redelivered = redelivered;
			this.RoutingKey = routingKey;
			this.AppId = appId;
			this.ClusterId = clusterId;
			this.ContentEncoding = contentEncoding;
			this.ContentType = contentType;
			this.CorrelationId = correlationId;
			this.DeliveryMode = deliveryMode;
			this.Expiration = expiration;
			this.MessageId = messageId;
			this.Priority = priority;
			this.ReplyTo = replyTo;
			this.Type = type;
			this.UserId = userId;
		}
	}
}
