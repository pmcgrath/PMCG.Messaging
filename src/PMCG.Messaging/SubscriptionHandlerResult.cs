using System;


namespace PMCG.Messaging
{
	public enum SubscriptionHandlerResult
	{
		None,
		Errored,
		Completed,
		Requeue
	}
}