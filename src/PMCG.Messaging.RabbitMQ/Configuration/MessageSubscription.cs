using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageSubscription
	{
		public readonly Type Type;
		public readonly string QueueName;
		public readonly string TypeHeader;
		public readonly Func<Message, MessageSubscriptionActionResult> Action;
		

		public MessageSubscription(
			Type type,
			string queueName,
			string typeHeader,
			Func<Message, MessageSubscriptionActionResult> action)
		{
			Check.RequireArgumentNotNull("type", type);
			Check.RequireArgument("type", type, type.IsSubclassOf(typeof(Message)));
			Check.RequireArgumentNotEmpty("queueName", queueName);
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);
			Check.RequireArgumentNotNull("action", action);

			this.Type = type;
			this.QueueName = queueName;
			this.TypeHeader = typeHeader;
			this.Action = action;
		}
	}
}