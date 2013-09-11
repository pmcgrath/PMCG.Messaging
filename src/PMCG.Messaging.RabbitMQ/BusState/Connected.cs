using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.RabbitMQ.BusState
{
	public class Connected : State
	{
		private readonly CancellationTokenSource c_cancellationTokenSource;
		private readonly Task[] c_publisherTasks;
		private readonly Subscriber c_subscriber;


		public Connected(
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(logger, configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info();

			this.c_cancellationTokenSource = new CancellationTokenSource();
			base.ConnectionManager.Disconnected += this.OnConnectionDisconnected;

			base.Logger.Info("About to create publisher tasks");
			this.c_publisherTasks = new Task[base.NumberOfPublishers];
			for (var _index = 0; _index < this.c_publisherTasks.Length; _index++)
			{
				this.c_publisherTasks[_index] = new Task(
					() =>
					{
						new Publisher(
							base.Logger,
							base.ConnectionManager.Connection,
							this.c_cancellationTokenSource.Token,
							base.QueuedMessages)
							.Start();
					},
					TaskCreationOptions.LongRunning);
				this.c_publisherTasks[_index].Start();
			}

			base.Logger.Info("About to create subscriber");
			this.c_subscriber = new Subscriber(
				base.Logger,
				base.ConnectionManager.Connection,
				base.MessageSubscriptions);
			this.c_subscriber.Start();

			base.Logger.Info("Completed");
		}


		public override void Close()
		{
			base.Logger.Info();

			this.c_subscriber.Stop();

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Completed");
		}


		public override void Publish<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("Publishing message ({0}) with Id {1}", message, message.Id);
			base.QueueMessageForDelivery(message);
			base.Logger.Info("Completed");
		}


		private void OnConnectionDisconnected(
			object sender,
			ConnectionDisconnectedEventArgs eventArgs)
		{
			base.Logger.InfoFormat("Connection has been disconnected for code ({0}) and reason ({1})", eventArgs.Code, eventArgs.Reason);

			this.c_subscriber.Stop();

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.TransitionToNewState(typeof(Disconnected));

			base.Logger.Info("Completed");
		}
	}
}