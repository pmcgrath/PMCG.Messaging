using System;


namespace PMCG.Messaging.Client.Interactive
{
	public class MyOtherEvent : Event
	{
		public readonly string Detail;
		public readonly int Number;


		public MyOtherEvent(
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
