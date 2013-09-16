using System;


namespace PMCG.Messaging.Client.Configuration
{
	public enum MessageDeliveryMode : byte
	{
		NonPersistent = 1,
		Persistent = 2
	}
}