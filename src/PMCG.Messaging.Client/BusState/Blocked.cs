using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.BusState
{
	public class Blocked : State
	{
		public Blocked(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Starting"); 
			
			base.ConnectionManager.Disconnected += this.OnConnectionDisconnected;
			base.ConnectionManager.Unblocked += this.OnConnectionUnblocked;

			base.Logger.Info("ctor Completed");
		}


		public override void Close()
		{
			base.Logger.Info("Close Starting");

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			base.ConnectionManager.Unblocked -= this.OnConnectionUnblocked;
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
				_result.SetResult(new PublicationResult(PublicationResultStatus.Blocked, message));
			}

			base.Logger.Debug("PublishAsync Completed");
			return _result.Task;
		}


		private void OnConnectionDisconnected(
			object sender,
			ConnectionDisconnectedEventArgs eventArgs)
		{
			base.Logger.InfoFormat("OnConnectionDisconnected Connection has been disconnected for code ({0}) and reason ({1})", eventArgs.Code, eventArgs.Reason);

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			base.ConnectionManager.Unblocked -= this.OnConnectionUnblocked;
			base.TransitionToNewState(typeof(Disconnected));

			base.Logger.Info("OnConnectionDisconnected Completed");
		}


		private void OnConnectionUnblocked(
			object sender,
			EventArgs eventArgs)
		{
			base.Logger.Info("OnConnectionUnblocked Connection has been unblocked");

			base.ConnectionManager.Disconnected -= this.OnConnectionDisconnected;
			base.ConnectionManager.Unblocked -= this.OnConnectionUnblocked;
			base.TransitionToNewState(typeof(Connected));

			base.Logger.Info("OnConnectionUnblocked Completed");
		}
	}
}