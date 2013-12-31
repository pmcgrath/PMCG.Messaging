using System;


namespace PMCG.Messaging.Client.Interactive
{
	public class Configuration
	{
		public const string LocalConnectionUri = "amqp://guest:guest@localhost:5672/";
		public const string ExchangeName1 = "test.exchange.1";
		public const string QueueName1 = "test.queue.1";
		public const string ExchangeName2 = "test.exchange.2";
		public const string QueueName2 = "test.queue.2";
	}
}
