using System;


namespace PMCG.Messaging.Client.UT
{
	public class MyCommand : Command
	{
		public readonly int Number;


		public MyCommand(
			Guid id,
			string correlationId,
			DateTimeOffset timestamp,
			int number)
			: base(id, correlationId, timestamp)
		{
			this.Number = number;
		}
	}
}
