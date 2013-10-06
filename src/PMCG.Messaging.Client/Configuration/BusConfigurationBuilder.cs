using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class BusConfigurationBuilder
	{
		public IList<string> ConnectionUris;
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
			this.SubscriptionDequeueTimeout = TimeSpan.FromMilliseconds(100);

			this.ConnectionUris = new List<string>();
			this.MessagePublications = new Dictionary<Type, List<MessageDelivery>>();
			this.MessageSubscriptions = new Dictionary<string, MessageSubscription>();
		}


		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName)
			where TMessage : Message
		{
			return this.RegisterPublication<TMessage>(
				exchangeName,
				typeof(TMessage).FullName);
		}

	
		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName,
			string typeHeader)
			where TMessage : Message
		{
			return this.RegisterPublication<TMessage>(
				exchangeName,
				typeHeader,
				MessageDeliveryMode.Persistent);
		}


		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName,
			string typeHeader,
			MessageDeliveryMode deliveryMode)
			where TMessage : Message
		{
			return this.RegisterPublication<TMessage>(
				exchangeName,
				typeHeader,
				deliveryMode,
				message => string.Empty);
		}


		public BusConfigurationBuilder RegisterPublication<TMessage>(
			string exchangeName,
			string typeHeader,
			MessageDeliveryMode deliveryMode,
			Func<Message, string> routingKeyFunc)
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
					typeHeader,
					deliveryMode,
					routingKeyFunc));

			return this;
		}


		public BusConfigurationBuilder RegisterSubscription<TMessage>(
			string queueName,
			string typeHeader,
			Func<TMessage, SubscriptionHandlerResult> action)
			where TMessage : Message
		{
			// Check that no exiting entry for the typeHeader

			Func<Message, SubscriptionHandlerResult> _actionWrapper = message => action(message as TMessage);

			this.MessageSubscriptions[typeHeader] = new MessageSubscription(
				typeof(TMessage),
				queueName,
				typeHeader,
				_actionWrapper);

			return this;
		}


		public BusConfigurationBuilder RegisterSubscription<TMessage>(
			string typeHeader,
			Func<TMessage, SubscriptionHandlerResult> action,
			string exchangeName)
			where TMessage : Message
		{
			// Check that no exiting entry for the typeHeader

			Func<Message, SubscriptionHandlerResult> _actionWrapper = message => action(message as TMessage);

			this.MessageSubscriptions[typeHeader] = new MessageSubscription(
				typeof(TMessage),
				typeHeader,
				_actionWrapper,
				exchangeName);

			return this;
		}



		public BusConfiguration Build()
		{
			return new BusConfiguration(
				this.ConnectionUris,
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