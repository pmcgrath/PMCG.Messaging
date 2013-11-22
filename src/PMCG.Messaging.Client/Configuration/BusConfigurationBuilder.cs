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
		public ushort NumberOfConsumers;
		public ushort ConsumerMessagePrefetchCount;
		public TimeSpan ConsumerDequeueTimeout;
		public IDictionary<Type, List<MessageDelivery>> MessagePublications;
		public IDictionary<string, MessageConsumer> MessageConsumers;


		public BusConfigurationBuilder()
		{
			this.ReconnectionPauseInterval = TimeSpan.FromSeconds(4);
			this.NumberOfConsumers = 1;
			this.ConsumerMessagePrefetchCount = 1;
			this.ConsumerDequeueTimeout = TimeSpan.FromMilliseconds(100);

			this.ConnectionUris = new List<string>();
			this.MessagePublications = new Dictionary<Type, List<MessageDelivery>>();
			this.MessageConsumers = new Dictionary<string, MessageConsumer>();
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

			if (!this.MessagePublications.ContainsKey(_messageType))
			{
				this.MessagePublications.Add(_messageType, new List<MessageDelivery>());
			}

			var _isCommandAndCommandEntryAlreadyExists = (typeof(Command).IsAssignableFrom(_messageType) && this.MessagePublications[_messageType].Count > 0);
			Check.Ensure(!_isCommandAndCommandEntryAlreadyExists, "Commands can only have one publication entry");

			this.MessagePublications[_messageType].Add(
				new MessageDelivery(
					exchangeName,
					typeHeader,
					deliveryMode,
					routingKeyFunc));

			return this;
		}


		public BusConfigurationBuilder RegisterConsumer<TMessage>(
			string queueName,
			string typeHeader,
			Func<TMessage, ConsumerHandlerResult> action)
			where TMessage : Message
		{
			// Check that no existing entry for the typeHeader

			Func<Message, ConsumerHandlerResult> _actionWrapper = message => action(message as TMessage);

			this.MessageConsumers[typeHeader] = new MessageConsumer(
				typeof(TMessage),
				queueName,
				typeHeader,
				_actionWrapper);

			return this;
		}


		public BusConfigurationBuilder RegisterConsumer<TMessage>(
			string typeHeader,
			Func<TMessage, ConsumerHandlerResult> action,
			string exchangeName)
			where TMessage : Message
		{
			// Check that no existing entry for the typeHeader

			Func<Message, ConsumerHandlerResult> _actionWrapper = message => action(message as TMessage);

			this.MessageConsumers[typeHeader] = new MessageConsumer(
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
				this.NumberOfConsumers,
				this.ConsumerMessagePrefetchCount,
				this.ConsumerDequeueTimeout,
				new MessagePublications(
					this.MessagePublications
						.Keys
						.Select(type => new MessagePublication(type, this.MessagePublications[type]))),
				new MessageConsumers(
					this.MessageConsumers.Values));
		}
	}
}