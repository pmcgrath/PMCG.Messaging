using System;


namespace PMCG.Messaging.Client.UT
{
	public class MyEvent : Event
	{
		public readonly string Detail;
		public readonly int Number;


		public MyEvent(
			Guid id,
			string correlationId,
			string detail,
			int number)
			: base(id, correlationId)
		{
			this.Detail = detail;
			this.Number = number;
		}
	}
}
