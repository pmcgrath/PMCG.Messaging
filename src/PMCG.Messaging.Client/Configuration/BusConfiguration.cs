using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.Configuration
{
	public class BusConfiguration
	{
		public readonly IEnumerable<string> ConnectionUris;
		public readonly TimeSpan HeartbeatInterval;
		public readonly TimeSpan ReconnectionPauseInterval;
		public readonly ushort NumberOfConsumers;
		public readonly ushort ConsumerMessagePrefetchCount;
		public readonly TimeSpan ConsumerDequeueTimeout;
		public readonly MessagePublications MessagePublications;
		public readonly MessageConsumers MessageConsumers;


		public BusConfiguration(
			IEnumerable<string> connectionUris,
			TimeSpan heartbeatInterval,
			TimeSpan reconnectionPauseInterval,
			ushort numberOfConsumers,
			ushort consumerMessagePrefetchCount,
			TimeSpan consumerDequeueTimeout,
			MessagePublications messagePublications,
			MessageConsumers messageConsumers)
		{
			Check.RequireArgumentNotEmptyAndNonEmptyItems("connectionUris", connectionUris);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.TotalSeconds > 0);
			Check.RequireArgument("numberOfConsumers", numberOfConsumers, numberOfConsumers > 0);
			Check.RequireArgument("consumerMessagePrefetchCount", consumerMessagePrefetchCount, consumerMessagePrefetchCount > 0);
			Check.RequireArgument("consumerDequeueTimeout", consumerDequeueTimeout, consumerDequeueTimeout.TotalSeconds > 0);
			Check.RequireArgumentNotNull("messagePublications", messagePublications);
			Check.RequireArgumentNotNull("messageConsumers", messageConsumers);
			
			this.ConnectionUris = connectionUris;
			this.HeartbeatInterval = heartbeatInterval;
			this.ReconnectionPauseInterval = reconnectionPauseInterval;
			this.NumberOfConsumers = numberOfConsumers;
			this.ConsumerMessagePrefetchCount = consumerMessagePrefetchCount;
			this.ConsumerDequeueTimeout = consumerDequeueTimeout;
			this.MessagePublications = messagePublications;
			this.MessageConsumers = messageConsumers;
		}
	}
}