using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using PMCG.Messaging.Client.Utility;
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
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(logger, configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info();
			
			this.c_disconnectedMessageStore = ServiceLocator.GetNewDisconnectedStore(base.Configuration);
			this.StoreDisconnectedMessages();
			new Task(this.TryRestablishingConnection).Start();

			base.Logger.Info("Completed");
		}


		public override void Close()
		{
			base.Logger.Info();
			base.CloseConnection();
			base.Logger.Info("Completed");
		}


		public override void Publish<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("Storing message ({0}) with Id {1}", message, message.Id);
			this.c_disconnectedMessageStore.Add(message);
			base.Logger.Info("Completed");
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
			base.Logger.Info();

			base.OpenConnection();
			base.RequeueDisconnectedMessages(this.c_disconnectedMessageStore);
			base.TransitionToNewState(typeof(Connected));

			base.Logger.Info("Completed");
		}
	}
}