using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Disconnected : State
	{
		private readonly IStore c_disconnectedMessageStore;


		public Disconnected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Starting");
			
			this.c_disconnectedMessageStore = ServiceLocator.GetNewDisconnectedStore(base.Configuration);
			new Task(this.TryRestablishingConnection).Start();

			base.Logger.Info("ctor Completed");
		}


		public override void Close()
		{
			base.Logger.Info("Close Starting");

			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Close Completed");
		}


		public override void Publish<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("Publish Storing message ({0}) with Id {1}", message, message.Id);
			this.c_disconnectedMessageStore.Add(message);
			base.Logger.Info("Publish Completed");
		}


		public override IEnumerable<Task<bool>> PublishAsync<TMessage>(
			TMessage message)
		{
			// TODO: Should we support this - if so how do we know the difference between sync and async when we re-connect
			return base.PublishAsync(message);
		}


		private void TryRestablishingConnection()
		{
			base.Logger.Info("TryRestablishingConnection Starting");

			base.OpenConnection();
			if (base.ConnectionManager.IsOpen)
			{
				base.TransitionToNewState(typeof(Connected));
			}

			base.Logger.Info("TryRestablishingConnection Completed");
		}
	}
}