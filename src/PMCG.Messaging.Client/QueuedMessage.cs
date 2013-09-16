using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client
{
	public class QueuedMessage
	{
		public readonly MessageDelivery Configuration;
		public readonly Message Data;


		public string ExchangeName { get { return this.Configuration.ExchangeName; } }
		public Byte DeliveryMode { get { return (byte)this.Configuration.DeliveryMode; } }
		public string RoutingKey { get { return this.Configuration.RoutingKeyFunc(this.Data); } }
		public string TypeHeader { get { return this.Configuration.TypeHeader; } }


		public QueuedMessage(
			MessageDelivery configuration,
			Message data)
		{
			this.Configuration = configuration;
			this.Data = data;
		}
	}
}