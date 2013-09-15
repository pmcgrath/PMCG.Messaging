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
			var _distinctMessages = base.QueuedMessages.Select(queuedMessage => queuedMessage.Data).Distinct().ToArray();
			this.c_disconnectedMessageStore.Store(_distinctMessages);
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

			var _queuedMessageIds = base.QueuedMessages
				.Select(queuedMessage => queuedMessage.Data.Id)
				.Distinct()
				.ToArray();

			foreach (var _messageKey in this.c_disconnectedMessageStore.GetAllMessageKeys())
			{
				var _messageId = this.c_disconnectedMessageStore.GetMessageIdFromKey(_messageKey);
				var _isDisconnectedMessageInQueue = _queuedMessageIds.Any(id => id == _messageId);
				if (!_isDisconnectedMessageInQueue)
				{
					var _message = this.c_disconnectedMessageStore.ReadMessage(_messageKey);
					base.QueueMessageForDelivery(_message);
				}

				this.c_disconnectedMessageStore.RemoveMessage(_messageKey);
			}

			base.Logger.Info("Completed");
		}
	}
}