using System.Linq;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Disconnected : State
	{
		private readonly IStore c_disconnectedMessageStore;


		public Disconnected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Starting");
			
			this.c_disconnectedMessageStore = ServiceLocator.GetNewDisconnectedStore(base.Configuration);
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

			this.c_disconnectedMessageStore.Add(message);

			var _queuedMessage = new QueuedMessage(
				this.Configuration.MessagePublications[message.GetType()].Configurations.First(),
				message);
			var _result = new TaskCompletionSource<PublicationResult[]>();
			_result.SetResult(new [] { new PublicationResult(_queuedMessage, PublicationResultStatus.Disconnected) });

			base.Logger.Info("PublishAsync Completed");
			return _result.Task;
		}


		private void TryRestablishingConnection()
		{
			base.Logger.Info("TryRestablishingConnection Starting");

			base.OpenConnection();
			if (base.ConnectionManager.IsOpen)
			{
				base.TransitionToNewState(typeof(Connected));
			}

			base.Logger.Info("TryRestablishingConnection Completed");
		}
	}
}