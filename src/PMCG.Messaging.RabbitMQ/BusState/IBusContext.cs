using System;


namespace PMCG.Messaging.RabbitMQ.BusState
{
	public interface IBusContext
	{
		State State { get; set; }
	}
}