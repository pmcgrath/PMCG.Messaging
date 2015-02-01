using log4net;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public abstract class State
	{
		protected readonly ILog Logger;
		protected readonly BusConfiguration Configuration;
		protected readonly IConnectionManager ConnectionManager;
		protected readonly IBusContext Context;


		protected uint NumberOfPublishers { get { return this.Configuration.NumberOfPublishers; } }
		protected uint NumberOfConsumers { get { return this.Configuration.NumberOfConsumers; } }


		protected State(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
		{
			this.Logger = LogManager.GetLogger(this.GetType());
			this.Logger.Info("ctor Starting");

			this.Configuration = configuration;
			this.ConnectionManager = connectionManager;
			this.Context = context;

			this.Logger.Info("ctor Completed");
		}


		public virtual void Connect()
		{
			throw new InvalidOperationException(string.Format("Connect operation is invalid for current state ({0})", this.GetType().Name));
		}


		public virtual void Close()
		{
			throw new InvalidOperationException(string.Format("Close operation is invalid for current state ({0})", this.GetType().Name));
		}


		public virtual Task<PMCG.Messaging.PublicationResult> PublishAsync<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			throw new InvalidOperationException(string.Format("Publish operation is invalid for current state ({0})", this.GetType().Name));
		}


		protected void TransitionToNewState(
			Type newState)
		{
			// Critical section - Lock should be okay as we do not expect contention here
			this.Logger.InfoFormat("TransitionToNewState Changing from {0} to {1}", this.Context.State.GetType().Name, newState.Name);
			this.Context.State = (State)Activator.CreateInstance(
				newState,
				new object[]
					{
						this.Configuration,
						this.ConnectionManager,
						this.Context
					});
			this.Logger.Info("TransitionToNewState Completed");
		}


		protected void OpenConnectionInitially()
		{
			this.ConnectionManager.Open(numberOfTimesToTry: 1);
		}


		protected void OpenConnection()
		{
			this.ConnectionManager.Open();
		}

	
		protected void CloseConnection()
		{
			this.ConnectionManager.Close();
		}


		protected bool DoesPublicationConfigurationExist<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			if (!this.Configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				this.Logger.WarnFormat("No configuration exists for publication of message ({0}) with Id {1}", message, message.Id);
				Check.Ensure(!typeof(Command).IsAssignableFrom(typeof(TMessage)), "Commands must have a publication configuration");
				return false;
			}

			return true;
		}
	}
}
