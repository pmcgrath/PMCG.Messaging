using System;


namespace PMCG.Messaging.Client.Interactive
{
	public class MyEvent : Event
	{
		public readonly string RunIdentifier;
		public readonly int Sequence;
		public readonly string Time;
		public readonly string Data;


		public MyEvent(
			Guid id,
			string correlationId,
			string runIdentifier,
			int sequence,
			string time,
			string data)
			: base(id, correlationId)
		{
			this.RunIdentifier = runIdentifier;
			this.Sequence = sequence;
			this.Time = time;
			this.Data = data;
		}
	}
}
