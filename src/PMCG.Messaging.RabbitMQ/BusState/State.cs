using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.RabbitMQ.BusState
{
	public abstract class State
	{
		protected readonly ILog Logger;
		protected readonly BusConfiguration Configuration;
		protected readonly IConnectionManager ConnectionManager;
		protected readonly BlockingCollection<QueuedMessage> QueuedMessages;
		protected readonly IBusContext Context;


		protected MessagePublications MessagePublications { get { return this.Configuration.MessagePublications; } }
		protected MessageSubscriptions MessageSubscriptions { get { return this.Configuration.MessageSubscriptions; } }
		protected uint NumberOfPublishers { get { return this.Configuration.NumberOfPublishers; } }


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

			this.Logger.Info("Completed");
		}


		public virtual void Connect()
		{
			this.Logger.Info();
		}


		public virtual void Close()
		{
			this.Logger.Info();
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
			this.Logger.InfoFormat("Changing from {0} to {1}", this.Context.State.GetType().Name, newState.Name);
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
			this.Logger.Info("Completed");
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
	}
}