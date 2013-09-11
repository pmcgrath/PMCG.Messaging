using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PMCG.Messaging.RabbitMQ.BusState
{
	public class Disconnected : State
	{
		private readonly DisconnectedMessageStore c_disconnectedMessageStore;


		public Disconnected(
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(logger, configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info();
			
			this.c_disconnectedMessageStore = new DisconnectedMessageStore(base.Configuration.DisconnectedMessagesStoragePath);
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
			this.c_disconnectedMessageStore.Store(message);
			base.Logger.Info("Completed");
		}


		private void StoreDisconnectedMessages()
		{
			var _queuedMessages = new List<QueuedMessage>();
			foreach (var _queuedMessage in base.QueuedMessages)
			{
				_queuedMessages.Add(_queuedMessage);
			}
			var _distinctMessages = _queuedMessages.Select(queuedMessage => queuedMessage.Data).Distinct();
			this.c_disconnectedMessageStore.Store(_distinctMessages.ToArray());
		}


		private void TryRestablishingConnection()
		{
			base.Logger.Info();

			base.OpenConnection();
			this.RequeueDisconnectedMessages();
			base.TransitionToNewState(typeof(Connected));

			base.Logger.Info("Completed");
		}


		private void RequeueDisconnectedMessages()
		{
			base.Logger.Info();

			var _disconnectedMessages = this.c_disconnectedMessageStore.GetAll(purgeMessages: true);
			foreach (var _message in _disconnectedMessages)
			{
				base.QueueMessageForDelivery(_message);
			}

			base.Logger.Info("Completed");
		}
	}
}