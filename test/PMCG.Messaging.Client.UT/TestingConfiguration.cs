using System;


namespace PMCG.Messaging.Client.UT
{
	public class TestingConfiguration
	{
		public const string LocalConnectionUri = "amqp://appuser:appuser@localhost:5672/";
		public const string ExchangeName = "test.exchange.1";
		public const string QueueName = "test.queue.1";
	}
}
