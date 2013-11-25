using System;


namespace PMCG.Messaging.Client
{
	public enum PublisherResultStatus
	{
		None,
		Acked,
		Nacked,
		ChannelShutdown
	}
}