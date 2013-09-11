using System;


namespace PMCG.Messaging.RabbitMQ.UT
{
	public class AnEvent : Event
	{
		public readonly string Detail;

		public AnEvent(
			Guid id,
			string detail)
			: base(id)
		{
			this.Detail = detail;
		}
	}
}
