using System;


namespace PMCG.Messaging.RabbitMQ
{
	public interface IMessageDeliveryHandler
	{
		void Handle(
			SubscriptionMessage subject);
	}
}