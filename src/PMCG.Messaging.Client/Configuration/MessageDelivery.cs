using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageDelivery
	{
		public readonly string ExchangeName;
		public readonly string TypeHeader;
		public readonly MessageDeliveryMode DeliveryMode;
		public readonly Func<Message, string> RoutingKeyFunc;


		public MessageDelivery(
			string exchangeName,
			string typeHeader,
			MessageDeliveryMode deliveryMode,
			Func<Message, string> routingKeyFunc)
		{
			Check.RequireArgumentNotEmpty("exchangeName", exchangeName);
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);
			Check.RequireArgument("deliveryMode", deliveryMode, Enum.IsDefined(typeof(MessageDeliveryMode), deliveryMode));
			Check.RequireArgumentNotNull("routingKeyFunc", routingKeyFunc);
			
			this.ExchangeName = exchangeName;
			this.TypeHeader = typeHeader;
			this.DeliveryMode = deliveryMode;
			this.RoutingKeyFunc = routingKeyFunc;
		}
	}
}