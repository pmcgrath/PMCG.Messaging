using System;


namespace PMCG.Messaging
{
	public interface IBus
	{
		void Publish<TMessage>(
			TMessage message)
			where TMessage : Message;
	}
}