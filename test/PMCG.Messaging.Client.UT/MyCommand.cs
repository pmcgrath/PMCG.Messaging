using System;


namespace PMCG.Messaging.Client.UT
{
	public class MyCommand : Command
	{
		public readonly int Number;


		public MyCommand(
			Guid id,
			string correlationId,
			int number)
			: base(id, correlationId)
		{
			this.Number = number;
		}
	}
}
