using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.Configuration
{
	public class BusConfiguration
	{
		public readonly IEnumerable<string> ConnectionUris;
		public readonly string DisconnectedMessagesStoragePath;
		public readonly TimeSpan ReconnectionPauseInterval;
		public readonly ushort NumberOfPublishers;
		public readonly ushort NumberOfSubscribers;
		public readonly ushort SubscriptionMessagePrefetchCount;
		public readonly TimeSpan SubscriptionDequeueTimeout;
		public readonly MessagePublications MessagePublications;
		public readonly MessageSubscriptions MessageSubscriptions;


		public BusConfiguration(
			IEnumerable<string> connectionUris,
			string disconnectedMessagesStoragePath,
			TimeSpan reconnectionPauseInterval,
			ushort numberOfPublishers,
			ushort numberOfSubscribers,
			ushort subscriptionMessagePrefetchCount,
			TimeSpan subscriptionDequeueTimeout,
			MessagePublications messagePublications,
			MessageSubscriptions messageSubscriptions)
		{
			Check.RequireArgumentNotEmptyAndNonEmptyItems("connectionUris", connectionUris);
			Check.RequireArgumentNotEmpty("disconnectedMessagesStoragePath", disconnectedMessagesStoragePath);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.TotalSeconds > 0);
			Check.RequireArgument("numberOfPublishers", numberOfPublishers, numberOfPublishers > 0);
			Check.RequireArgument("numberOfSubscribers", numberOfSubscribers, numberOfSubscribers > 0);
			Check.RequireArgument("subscriptionMessagePrefetchCount", subscriptionMessagePrefetchCount, subscriptionMessagePrefetchCount > 0);
			Check.RequireArgument("subscriptionDequeueTimeout", subscriptionDequeueTimeout, subscriptionDequeueTimeout.TotalSeconds > 0);
			Check.RequireArgumentNotNull("messagePublications", messagePublications);
			Check.RequireArgumentNotNull("messageSubscriptions", messageSubscriptions);
			
			this.ConnectionUris = connectionUris;
			this.DisconnectedMessagesStoragePath = disconnectedMessagesStoragePath;
			this.ReconnectionPauseInterval = reconnectionPauseInterval;
			this.NumberOfPublishers = numberOfPublishers;
			this.NumberOfSubscribers = numberOfSubscribers;
			this.SubscriptionMessagePrefetchCount = subscriptionMessagePrefetchCount;
			this.SubscriptionDequeueTimeout = subscriptionDequeueTimeout;
			this.MessagePublications = messagePublications;
			this.MessageSubscriptions = messageSubscriptions;
		}
	}
}