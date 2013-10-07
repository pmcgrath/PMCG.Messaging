using System;


namespace PMCG.Messaging
{
	public abstract class Message
	{
		public readonly Guid Id;
		public readonly string CorrelationId;


		protected Message(
			Guid id,
			string correlationId)
		{
			this.Id = id;
			this.CorrelationId = correlationId;
		}
	}
}