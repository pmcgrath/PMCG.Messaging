using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Disconnected : State
	{
		private readonly IStore c_disconnectedMessageStore;


		public Disconnected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info("ctor Starting");
			
			this.c_disconnectedMessageStore = ServiceLocator.GetNewDisconnectedStore(base.Configuration);
			this.StoreDisconnectedMessages();
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


		private void StoreDisconnectedMessages()
		{
			var _distinctMessages = base.QueuedMessages.Select(queuedMessage => queuedMessage.Data).Distinct();
			foreach (var _message in _distinctMessages)
			{
				this.c_disconnectedMessageStore.Add(_message);
			}
		}


		private void TryRestablishingConnection()
		{
			base.Logger.Info("TryRestablishingConnection Starting");

			base.OpenConnection();
			if (base.ConnectionManager.IsOpen)
			{
				base.RequeueDisconnectedMessages(this.c_disconnectedMessageStore);
				base.TransitionToNewState(typeof(Connected));
			}

			base.Logger.Info("TryRestablishingConnection Completed");
		}
	}
}