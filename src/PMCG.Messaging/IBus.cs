using System;
using System.Threading.Tasks;


namespace PMCG.Messaging
{
	public interface IBus
	{
		Task<PublicationResult> PublishAsync<TMessage>(
			TMessage message)
			where TMessage : Message;
	}
}