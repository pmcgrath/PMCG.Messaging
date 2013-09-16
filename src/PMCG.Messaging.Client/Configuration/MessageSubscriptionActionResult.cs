using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public enum MessageSubscriptionActionResult
	{
		None,
		Errored,
		Completed,
		Requeue
	}
}