using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public enum MessageDeliveryMode : byte
	{
		NonPersistent = 1,
		Persistent = 2
	}
}