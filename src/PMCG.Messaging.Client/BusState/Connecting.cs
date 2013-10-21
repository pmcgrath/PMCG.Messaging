using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.BusState
{
	public class Connecting : State
	{
		public Connecting(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			IBusContext context)
			: base(configuration, connectionManager, context)
		{
			base.Logger.Info("ctor Completed");
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