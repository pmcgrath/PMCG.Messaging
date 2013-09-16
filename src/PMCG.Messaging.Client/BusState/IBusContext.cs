using System;


namespace PMCG.Messaging.Client.BusState
{
	public interface IBusContext
	{
		State State { get; set; }
	}
}