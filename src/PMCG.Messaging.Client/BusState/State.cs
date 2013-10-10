using Common.Logging;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Concurrent;
using System.Linq;


namespace PMCG.Messaging.Client.BusState
{
	public abstract class State
	{
		protected readonly ILog Logger;
		protected readonly BusConfiguration Configuration;
		protected readonly IConnectionManager ConnectionManager;
		protected readonly BlockingCollection<QueuedMessage> QueuedMessages;
		protected readonly IBusContext Context;


		protected uint NumberOfPublishers { get { return this.Configuration.NumberOfPublishers; } }
		protected uint NumberOfConsumers { get { return this.Configuration.NumberOfConsumers; } }


		protected State(
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
		{
			this.Logger = logger;
			this.Configuration = configuration;
			this.ConnectionManager = connectionManager;
			this.QueuedMessages = queuedMessages;
			this.Context = context;

			this.Logger.Info("ctor Completed");
		}


		public virtual void Connect()
		{
			this.Logger.Info("Connect Completed - no override");
		}


		public virtual void Close()
		{
			this.Logger.Info("Close Completed - no override");
		}


		public virtual void Publish<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			throw new InvalidOperationException(string.Format("Publish is invalid for current state ({0})", this.GetType().Name));
		}


		protected void TransitionToNewState(
			Type newState)
		{
			this.Logger.InfoFormat("TransitionToNewState Changing from {0} to {1}", this.Context.State.GetType().Name, newState.Name);
			this.Context.State = (State)Activator.CreateInstance(
				newState,
				new object[]
					{
						this.Logger,
						this.Configuration,
						this.ConnectionManager,
						this.QueuedMessages,
						this.Context 
					});
			this.Logger.Info("TransitionToNewState Completed");
		}


		protected void OpenConnectionInitially()
		{
			this.ConnectionManager.Open(1);
		}


		protected void OpenConnection()
		{
			this.ConnectionManager.Open();
		}

	
		protected void CloseConnection()
		{
			this.ConnectionManager.Close();
		}


		protected void QueueMessageForDelivery(
			Message message)
		{
			if (this.Configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				foreach (var _deliveryConfiguration in this.Configuration.MessagePublications[message.GetType()].Configurations)
				{
					var _queuedMessage = new QueuedMessage(_deliveryConfiguration, message);
					this.QueuedMessages.Add(_queuedMessage);
				}
			}
		}


		protected void RequeueDisconnectedMessages(
			IStore disconnectedMessageStore)
		{
			this.Logger.Info("RequeueDisconnectedMessages Starting");

			var _queuedMessageIds = this.QueuedMessages
				.Select(queuedMessage => queuedMessage.Data.Id)
				.Distinct()
				.ToArray();

			foreach (var _messageId in disconnectedMessageStore.GetAllIds())
			{
				var _isDisconnectedMessageInQueue = _queuedMessageIds.Any(id => id == _messageId);
				if (!_isDisconnectedMessageInQueue)
				{
					var _message = disconnectedMessageStore.Get(_messageId);
					this.QueueMessageForDelivery(_message);
				}

				disconnectedMessageStore.Delete(_messageId);
			}

			this.Logger.Info("RequeueDisconnectedMessages Completed");
		}
	}
}