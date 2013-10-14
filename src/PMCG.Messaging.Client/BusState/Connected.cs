using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Connected : State
	{
		private readonly CancellationTokenSource c_cancellationTokenSource;
		private readonly Task[] c_publisherTasks;
		private readonly Task[] c_consumerTasks;


		public Connected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info("ctor Starting");

			this.c_cancellationTokenSource = new CancellationTokenSource();
			base.ConnectionManager.Disconnected += this.OnConnectionDisconnected;

			base.Logger.Info("ctor About to create publisher tasks");
			this.c_publisherTasks = new Task[base.NumberOfPublishers];
			for (var _index = 0; _index < this.c_publisherTasks.Length; _index++)
			{
				this.c_publisherTasks[_index] = new Task(
					() =>
					{
						new Publisher(
							base.ConnectionManager.Connection,
							this.c_cancellationTokenSource.Token,
							base.QueuedMessages)
							.Start();
					},
					TaskCreationOptions.LongRunning);
				this.c_publisherTasks[_index].Start();
			}

			base.Logger.Info("ctor About to reqeue disconnected messages");
			base.RequeueDisconnectedMessages(ServiceLocator.GetNewDisconnectedStore(base.Configuration));

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
			base.QueueMessageForDelivery(message);
			base.Logger.Info("Publish Completed");
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