using System;


namespace PMCG.Messaging
{
	public enum ConsumerHandlerResult
	{
		None,
		Errored,
		Completed,
		Requeue
	}
}