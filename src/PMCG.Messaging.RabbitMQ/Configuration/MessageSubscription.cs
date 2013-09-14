using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Diagnostics;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageSubscription
	{
		public readonly Type Type;
		public readonly string QueueName;
		public readonly string TypeHeader;
		public readonly Func<Message, MessageSubscriptionActionResult> Action;
		public readonly string ExchangeName;


		public bool UseTransientQueue { get { return this.ExchangeName != null; } }


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
			this.ExchangeName = null;
		}


		public MessageSubscription(
			Type type,
			string typeHeader,
			Func<Message, MessageSubscriptionActionResult> action,
			string exchangeName)
		{
			Check.RequireArgumentNotNull("type", type);
			Check.RequireArgument("type", type, type.IsSubclassOf(typeof(Message)));
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);
			Check.RequireArgumentNotNull("action", action);
			Check.RequireArgumentNotEmpty("exchangeName", exchangeName);

			this.Type = type;
			this.TypeHeader = typeHeader;
			this.Action = action;
			this.ExchangeName = exchangeName;

			// In case of a transient queue, we want a deterministic queue name
			this.QueueName = string.Format("{0}_{1}_{2}_{3}",
				Environment.MachineName,
				Process.GetCurrentProcess().Id,
				AppDomain.CurrentDomain.Id,
				this.ExchangeName);
		}
	}
}