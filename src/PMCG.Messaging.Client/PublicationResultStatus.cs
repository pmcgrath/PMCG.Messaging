using System;


namespace PMCG.Messaging.Client
{
	public enum PublicationResultStatus
	{
		None,
		Acked,
		Nacked,
		ChannelShutdown,
		Disconnected
	}
}