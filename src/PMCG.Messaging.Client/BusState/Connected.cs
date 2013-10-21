using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Connected : State
	{
		private readonly CancellationTokenSource c_cancellationTokenSource;
		private readonly Publisher c_publisher;
		private readonly Task[] c_consumerTasks;


		public Connected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Starting");

			this.c_cancellationTokenSource = new CancellationTokenSource();
			base.ConnectionManager.Disconnected += this.OnConnectionDisconnected;

			base.Logger.Info("ctor About to create publisher");
			this.c_publisher = new Publisher(base.ConnectionManager.Connection, base.Configuration.PublicationTimeout, this.c_cancellationTokenSource.Token);

			base.Logger.Info("ctor About to reqeue disconnected messages");
			this.RequeueDisconnectedMessages(ServiceLocator.GetNewDisconnectedStore(base.Configuration));

			base.Logger.Info("ctor About to create subcriber tasks");
			this.c_consumerTasks = new Task[base.NumberOfConsumers];
			for (var _index = 0; _index < this.c_consumerTasks.Length; _index++)
			{
				this.c_consumerTasks[_index] = new Task(
					() =>
					{
						new Consumer(
							base.ConnectionManager.Connection,
							base.Configuration,
							this.c_cancellationTokenSource.Token)
							.Start();
					},
					TaskCreationOptions.LongRunning);
				this.c_consumerTasks[_index].Start();
			}

			base.Logger.Info("ctor Completed");
		}


		public override void Close()
		{
			base.Logger.Info("Close Starting");

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Close Completed");
		}


		public override void Publish<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("Publish Publishing message ({0}) with Id {1}", message, message.Id);
			
			if (base.Configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				foreach (var _deliveryConfiguration in this.Configuration.MessagePublications[message.GetType()].Configurations)
				{
					var _queuedMessage = new QueuedMessage(_deliveryConfiguration, message);
					this.c_publisher.Publish(_queuedMessage);
				}
			}
			else
			{
				base.Logger.WarnFormat("No configuration exists for publication of message ({0}) with Id {1}", message, message.Id);
				Check.Ensure(typeof(TMessage).IsAssignableFrom(typeof(Command)), "Commands must have a publication configuration");
			}
			
			base.Logger.Info("Publish Completed");
		}


		public override IEnumerable<Task<bool>> PublishAsync<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("PublishAsync Publishing message ({0}) with Id {1}", message, message.Id);
			
			var _result = new List<Task<bool>>();
			if (base.Configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				foreach (var _deliveryConfiguration in this.Configuration.MessagePublications[message.GetType()].Configurations)
				{
					var _queuedMessage = new QueuedMessage(_deliveryConfiguration, message);
					var _publictionResult = this.c_publisher.PublishAsync(_queuedMessage);
					_result.Add(_publictionResult);
				}
			}
			else
			{
				base.Logger.WarnFormat("No configuration exists for publication of message ({0}) with Id {1}", message, message.Id);
				Check.Ensure(typeof(TMessage).IsAssignableFrom(typeof(Command)), "Commands must have a publication configuration");
			}

			base.Logger.Info("PublishAsync Completed");
			return _result;
		}


		private void RequeueDisconnectedMessages(
			IStore disconnectedMessageStore)
		{
			this.Logger.Info("RequeueDisconnectedMessages Starting");

			foreach (var _messageId in disconnectedMessageStore.GetAllIds())
			{
				var _message = disconnectedMessageStore.Get(_messageId);
				this.Publish(_message);
				disconnectedMessageStore.Delete(_messageId);
			}

			this.Logger.Info("RequeueDisconnectedMessages Completed");
		}


		private void OnConnectionDisconnected(
			object sender,
			ConnectionDisconnectedEventArgs eventArgs)
		{
			base.Logger.InfoFormat("OnConnectionDisconnected Connection has been disconnected for code ({0}) and reason ({1})", eventArgs.Code, eventArgs.Reason);

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.TransitionToNewState(typeof(Disconnected));

			base.Logger.Info("OnConnectionDisconnected Completed");
		}
	}
}