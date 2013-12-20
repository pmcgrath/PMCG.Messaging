﻿using PMCG.Messaging.Client.Configuration;
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
			base.ConnectionManager.Blocked += this.OnConnectionBlocked;					// Only seems to get fired if a publication is attempted
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


		public override Task<PublicationResult> PublishAsync<TMessage>(
			TMessage message)
		{
			base.Logger.DebugFormat("PublishAsync Publishing message ({0}) with Id {1}", message, message.Id);

			var _result = new TaskCompletionSource<PublicationResult>();
			if (!base.DoesPublicationConfigurationExist(message))
			{
				_result.SetResult(new PublicationResult(PublicationResultStatus.NoConfigurationFound, message));
			}
			else
			{
				var _queuedMessages = this.Configuration.MessagePublications[message.GetType()]
					.Configurations
					.Select(deliveryConfiguration => new QueuedMessage(deliveryConfiguration, message));

				// Could use Parallel.ForEach(_queuedMessages, queuedMessage => _tasks.Add(this.c_publisher.PublishAsync(queuedMessage)));
				// but we expect that there will only one publication for each message in most cases
				var _tasks = new List<Task<PublisherResult>>();
				foreach (var _queuedMessage in _queuedMessages) { _tasks.Add(this.c_publisher.PublishAsync(_queuedMessage)); }
				Task.WhenAll(_tasks).ContinueWith(taskResults =>
					{
						if (taskResults.IsFaulted) { _result.SetException(taskResults.Exception); }
						else { _result.SetResult(this.CreateNonFaultedPublicationResult(message, taskResults)); }
					});
			}

			base.Logger.Debug("PublishAsync Completed");
			return _result.Task;
		}


		private void OnConnectionBlocked(
			object sender,
			ConnectionBlockedEventArgs eventArgs)
		{
			base.Logger.InfoFormat("OnConnectionBlocked Connection has been blocked for reason ({0})", eventArgs.Reason);

			base.ConnectionManager.Blocked -= this.OnConnectionBlocked;
			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.TransitionToNewState(typeof(Blocked));

			base.Logger.Info("OnConnectionBlocked Completed");
		}


		private void OnConnectionDisconnected(
			object sender,
			ConnectionDisconnectedEventArgs eventArgs)
		{
			base.Logger.InfoFormat("OnConnectionDisconnected Connection has been disconnected for code ({0}) and reason ({1})", eventArgs.Code, eventArgs.Reason);

			base.ConnectionManager.Blocked -= this.OnConnectionBlocked;
			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			base.TransitionToNewState(typeof(Disconnected));

			base.Logger.Info("OnConnectionDisconnected Completed");
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


		private PublicationResult CreateNonFaultedPublicationResult(
			Message message,
			Task<PublisherResult[]> publisherResults)
		{
			var _allGood = publisherResults.Result.All(result => result.Status == PublisherResultStatus.Acked);
			var _status = _allGood ? PublicationResultStatus.Published : PublicationResultStatus.NotPublished;

			return new PublicationResult(_status, message);
		}
	}
}