using System;


namespace PMCG.Messaging
{
	public abstract class Command : Message
	{
		public readonly DateTimeOffset Timestamp;


		protected Command(
			Guid id,
			string correlationId,
			DateTimeOffset timestamp)
			: base(id, correlationId)
		{
			this.Timestamp = timestamp;
		}
	}
}