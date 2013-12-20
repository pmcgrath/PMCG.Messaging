using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Disconnected : State
	{
		public Disconnected(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Starting");
			
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


		public override Task<PublicationResult> PublishAsync<TMessage>(
			TMessage message)
		{
			base.Logger.DebugFormat("PublishAsync Storing message ({0}) with Id {1}", message, message.Id);

			var _result = new TaskCompletionSource<PublicationResult>();
			if (!base.DoesPublicationConfigurationExist(message))
			{
				_result.SetResult(new PublicationResult(PublicationResultStatus.NoConfigurationFound, message));
			}
			else
			{
				_result.SetResult(new PublicationResult(PublicationResultStatus.Disconnected, message));
			}

			base.Logger.Debug("PublishAsync Completed");
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