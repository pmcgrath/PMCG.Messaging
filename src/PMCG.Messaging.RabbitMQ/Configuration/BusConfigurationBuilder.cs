using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class BusConfigurationBuilder
	{
		public string ConnectionUri;
		public string DisconnectedMessagesStoragePath;
		public TimeSpan ReconnectionPauseInterval;
		public ushort NumberOfPublishers;
		public ushort NumberOfSubscribers;
		public ushort SubscriptionMessagePrefetchCount;
		public TimeSpan SubscriptionDequeueTimeout;
		public IDictionary<Type, List<MessageDelivery>> MessagePublications;
		public IDictionary<string, MessageSubscription> MessageSubscriptions;


		public BusConfigurationBuilder()
		{
			this.ReconnectionPauseInterval = TimeSpan.FromSeconds(4);
			this.NumberOfPublishers = 1;
			this.NumberOfSubscribers = 1;
			this.SubscriptionMessagePrefetchCount = 1;
			this.SubscriptionDequeueTimeout = TimeSpan.FromMilliseconds(500);

			this.MessagePublications = new Dictionary<Type, List<MessageDelivery>>();
			this.MessageSubscriptions = new Dictionary<string, MessageSubscription>();
		}


		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName,
			MessageDeliveryMode deliveryMode)
			where TMessage : Message
		{
			return this.RegisterPublication<TMessage>(
				exchangeName,
				deliveryMode,
				message => string.Empty,
				typeof(TMessage).FullName);
		}


		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName,
			MessageDeliveryMode deliveryMode,
			Func<Message, string> routingKeyFunc,
			string typeHeader)
			where TMessage : Message
		{
			var _messageType = typeof(TMessage);

			// Pending - check if exchange entry already exists

			if (!this.MessagePublications.ContainsKey(_messageType))
			{
				this.MessagePublications.Add(_messageType, new List<MessageDelivery>());
			}

			this.MessagePublications[_messageType].Add(
				new MessageDelivery(
					exchangeName,
					deliveryMode,
					routingKeyFunc,
					typeHeader));

			return this;
		}


		public BusConfigurationBuilder RegisterSubscription<TMessage>(
			string queueName,
			string typeHeader,
			Func<TMessage, MessageSubscriptionActionResult> action)
			where TMessage : Message
		{
			// Check that no exiting entry for the typeHeader

			Func<Message, MessageSubscriptionActionResult> _actionWrapper = message => action(message as TMessage);

			this.MessageSubscriptions[typeHeader] = new MessageSubscription(
				typeof(TMessage),
				queueName,
				typeHeader,
				_actionWrapper);

			return this;
		}


		public BusConfiguration Build()
		{
			return new BusConfiguration(
				this.ConnectionUri,
				this.DisconnectedMessagesStoragePath,
				this.ReconnectionPauseInterval,
				this.NumberOfPublishers,
				this.NumberOfSubscribers,
				this.SubscriptionMessagePrefetchCount,
				this.SubscriptionDequeueTimeout,
				new MessagePublications(
					this.MessagePublications
						.Keys
						.Select(type => new MessagePublication(type, this.MessagePublications[type]))),
				new MessageSubscriptions(
					this.MessageSubscriptions.Values));
		}
	}
}