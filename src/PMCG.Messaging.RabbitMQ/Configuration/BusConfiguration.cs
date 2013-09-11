using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class BusConfiguration
	{
		public readonly string ConnectionUri;
		public readonly string DisconnectedMessagesStoragePath;
		public readonly TimeSpan ReconnectionPauseInterval;
		public readonly uint NumberOfPublishers;
		public readonly MessagePublications MessagePublications;
		public readonly MessageSubscriptions MessageSubscriptions;


		public BusConfiguration(
			string connectionUri,
			string disconnectedMessagesStoragePath,
			TimeSpan reconnectionPauseInterval,
			uint numberOfPublishers,
			MessagePublications messagePublications,
			MessageSubscriptions messageSubscriptions)
		{
			Check.RequireArgumentNotEmpty("connectionUri", connectionUri);
			Check.RequireArgumentNotEmpty("disconnectedMessagesStoragePath", disconnectedMessagesStoragePath);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.TotalSeconds > 0);
			Check.RequireArgument("numberOfPublishers", numberOfPublishers, numberOfPublishers > 0U);
			Check.RequireArgumentNotNull("messagePublications", messagePublications);
			Check.RequireArgumentNotNull("messageSubscriptions", messageSubscriptions);
			
			this.ConnectionUri = connectionUri;
			this.DisconnectedMessagesStoragePath = disconnectedMessagesStoragePath;
			this.ReconnectionPauseInterval = reconnectionPauseInterval;
			this.NumberOfPublishers = numberOfPublishers;
			this.MessagePublications = messagePublications;
			this.MessageSubscriptions = messageSubscriptions;
		}
	}
}