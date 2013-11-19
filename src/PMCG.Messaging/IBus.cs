using System;
using System.Threading.Tasks;


namespace PMCG.Messaging
{
	public interface IBus
	{
		Task PublishAsync<TMessage>(
			TMessage message)
			where TMessage : Message;
	}
}