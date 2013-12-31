using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Connected : State
	{
		private readonly CancellationTokenSource c_cancellationTokenSource;
		private readonly BlockingCollection<Publication> c_publicationQueue;
		private readonly Task[] c_publisherTasks;
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

			this.c_publicationQueue = new BlockingCollection<Publication>();

			base.Logger.Info("ctor About to create publisher");
			this.c_publisherTasks = new Task[base.NumberOfPublishers];
			for (var _index = 0; _index < this.c_publisherTasks.Length; _index++)
			{
				var _publisher = new Publisher(base.ConnectionManager.Connection, this.c_publicationQueue, this.c_cancellationTokenSource.Token);
				this.c_publisherTasks[_index] = _publisher.Start();
			}

			base.Logger.Info("ctor About to create consumer tasks");
			this.c_consumerTasks = new Task[base.NumberOfConsumers];
			for (var _index = 0; _index < this.c_consumerTasks.Length; _index++)
			{
				var _consumer = new Consumer(base.ConnectionManager.Connection, base.Configuration, this.c_cancellationTokenSource.Token);
				this.c_consumerTasks[_index] = _consumer.Start();
			}

			base.Logger.Info("ctor Completed");
		}


		public override void Close()
		{
			base.Logger.Info("Close Starting");

			this.PrepareForTransition(PublicationResultStatus.Closed);
			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Close Completed");
		}


		public override Task<PMCG.Messaging.PublicationResult> PublishAsync<TMessage>(
			TMessage message)
		{
			base.Logger.DebugFormat("PublishAsync Publishing message ({0}) with Id {1}", message, message.Id);

			var _result = new TaskCompletionSource<PMCG.Messaging.PublicationResult>();
			if (this.c_cancellationTokenSource.IsCancellationRequested)
			{
				_result.SetResult(
					new PMCG.Messaging.PublicationResult(
						PMCG.Messaging.PublicationResultStatus.NotPublished,
						message));
			}
			else if (!base.DoesPublicationConfigurationExist(message))
			{
				_result.SetResult(
					new PMCG.Messaging.PublicationResult(
						PMCG.Messaging.PublicationResultStatus.NoConfigurationFound,
						message));
			}
			else
			{
				var _thisPublicationsPublications = this.Configuration.MessagePublications[message.GetType()]
					.Configurations
					.Select(deliveryConfiguration =>
						new Publication(
							deliveryConfiguration,
							message,
							new TaskCompletionSource<PublicationResult>()));

				var _tasks = new List<Task<PublicationResult>>();
				foreach (var _publication in _thisPublicationsPublications)
				{
					this.c_publicationQueue.Add(_publication);
					_tasks.Add(_publication.ResultTask);
				}
				Task.WhenAll(_tasks).ContinueWith(taskResults =>
					{
						if (taskResults.IsFaulted)	{ _result.SetException(taskResults.Exception); }
						else						{ _result.SetResult(this.CreateNonFaultedPublicationResult(message, taskResults)); }
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

			this.PrepareForTransition(PublicationResultStatus.ConnectionBlocked);
			base.TransitionToNewState(typeof(Blocked));

			base.Logger.Info("OnConnectionBlocked Completed");
		}


		private void OnConnectionDisconnected(
			object sender,
			ConnectionDisconnectedEventArgs eventArgs)
		{
			base.Logger.WarnFormat("OnConnectionDisconnected Connection has been disconnected for code ({0}) and reason ({1})", eventArgs.Code, eventArgs.Reason);

			this.PrepareForTransition(PublicationResultStatus.ConnectionDisconnected);
			base.TransitionToNewState(typeof(Disconnected));

			base.Logger.Warn("OnConnectionDisconnected Completed");
		}


		private void PrepareForTransition(
			PublicationResultStatus preEmptiveCompletionStatus)
		{
			base.ConnectionManager.Blocked -= this.OnConnectionBlocked;
			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			this.c_cancellationTokenSource.Cancel();
			this.c_publicationQueue.CompleteAdding();
			foreach (var _publication in this.c_publicationQueue) { _publication.SetResult(status); }
		}


		private PMCG.Messaging.PublicationResult CreateNonFaultedPublicationResult(
			Message message,
			Task<PublicationResult[]> publicationResults)
		{
			var _allGood = publicationResults.Result.All(result => result.Status == PublicationResultStatus.Acked);
			var _status = _allGood ? PMCG.Messaging.PublicationResultStatus.Published : PMCG.Messaging.PublicationResultStatus.NotPublished;

			return new PMCG.Messaging.PublicationResult(_status, message);
		}
	}
}