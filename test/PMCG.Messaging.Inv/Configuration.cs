using System;


namespace PMCG.Messaging.Inv
{
	public class Configuration
	{
		public const string LocalConnectionUri = "amqp://appuser:appuser@localhost:5672/";
		public const string ExchangeName = "test.exchange.1";
		public const string QueueName = "test.queue.1";
	}
}
