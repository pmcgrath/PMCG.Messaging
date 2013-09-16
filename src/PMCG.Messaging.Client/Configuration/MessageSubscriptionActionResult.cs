using System;


namespace PMCG.Messaging.Client.Configuration
{
	public enum MessageSubscriptionActionResult
	{
		None,
		Errored,
		Completed,
		Requeue
	}
}