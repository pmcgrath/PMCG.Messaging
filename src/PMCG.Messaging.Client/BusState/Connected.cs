using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Generic;
using System.Linq;
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
			this.c_publisher = new Publisher(base.ConnectionManager.Connection, this.c_cancellationTokenSource.Token);

			base.Logger.Info("ctor About to requeue disconnected messages");
			// Wrap in try catch - so we do not prevent starting - how long will this take ?
			// Also this is a problem if a lot of disconnected messages - will slow the the ctor completion, so any calls to publish will fail while this is running
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


		public override Task<PublicationResult[]> PublishAsync<TMessage>(
			TMessage message)
		{
			base.Logger.InfoFormat("PublishAsync Publishing message ({0}) with Id {1}", message, message.Id);
			
			if (!base.Configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				base.Logger.WarnFormat("No configuration exists for publication of message ({0}) with Id {1}", message, message.Id);
				Check.Ensure(!typeof(Command).IsAssignableFrom(typeof(TMessage)), "Commands must have a publication configuration");

				var _noConfigurationResult = new TaskCompletionSource<PublicationResult[]>();
				_noConfigurationResult.SetResult(new PublicationResult[0]);

				base.Logger.Info("PublishAsync Completed");
				return _noConfigurationResult.Task;
			}

			var _queuedMessages = this.Configuration.MessagePublications[message.GetType()]
				.Configurations
				.Select(deliveryConfiguration => new QueuedMessage(deliveryConfiguration, message));

			var _tasks = new List<Task<PublicationResult>>();
			Parallel.ForEach(_queuedMessages, queuedMessage => _tasks.Add(this.c_publisher.PublishAsync(queuedMessage)));

			var _result = Task.WhenAll(_tasks);

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
				this.PublishAsync(_message);
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