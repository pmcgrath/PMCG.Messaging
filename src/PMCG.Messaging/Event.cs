using System;


namespace PMCG.Messaging
{
	public abstract class Event : Message
	{
		protected Event(
			Guid id)
			: base(id)
		{
		}
	}
}