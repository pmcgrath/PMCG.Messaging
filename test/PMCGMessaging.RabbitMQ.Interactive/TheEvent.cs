using PMCG.Messaging;
using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	public class TheEvent : Event
	{
		public readonly string Detail;

		public TheEvent(
			Guid id,
			string detail)
			: base(id)
		{
			this.Detail = detail;
		}
	}
}
