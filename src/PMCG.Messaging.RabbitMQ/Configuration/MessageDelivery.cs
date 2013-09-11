using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageDelivery
	{
		public readonly string ExchangeName;
		public readonly MessageDeliveryMode DeliveryMode;
		public readonly Func<Message, string> RoutingKeyFunc;
		public readonly string TypeHeader;


		public MessageDelivery(
			string exchangeName,
			MessageDeliveryMode deliveryMode,
			Func<Message, string> routingKeyFunc,
			string typeHeader)
		{
			Check.RequireArgumentNotEmpty("exchangeName", exchangeName);
			Check.RequireArgument("deliveryMode", deliveryMode, Enum.IsDefined(typeof(MessageDeliveryMode), deliveryMode));
			Check.RequireArgumentNotNull("routingKeyFunc", routingKeyFunc);
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);

			this.ExchangeName = exchangeName;
			this.DeliveryMode = deliveryMode;
			this.RoutingKeyFunc = routingKeyFunc;
			this.TypeHeader = typeHeader;
		}
	}
}