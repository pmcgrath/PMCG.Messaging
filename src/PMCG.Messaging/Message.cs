using System;


namespace PMCG.Messaging
{
	public abstract class Message
	{
		public readonly Guid Id;


		protected Message(
			Guid id)
		{
			this.Id = id;
		}
	}
}