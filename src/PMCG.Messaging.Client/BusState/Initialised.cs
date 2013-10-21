using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.BusState
{
	public class Initialised : State
	{
		public Initialised(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Completed");
		}


		public override void Connect()
		{
			base.Logger.Info("Connect Starting");
			
			base.TransitionToNewState(typeof(Connecting));
			base.OpenConnectionInitially();
			base.TransitionToNewState(base.ConnectionManager.IsOpen ? typeof(Connected) : typeof(Disconnected));

			base.Logger.Info("Connect Completed");
		}


		public override void Close()
		{
			base.Logger.Info("Close Starting");

			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Close Completed");
		}
	}
}