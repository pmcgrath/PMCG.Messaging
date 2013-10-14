using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.Client.BusState
{
	public class Closed : State
	{
		public Closed(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info("ctor Completed");
		}


		public override void Connect()
		{
			base.Logger.Info("Connect Starting");

			base.TransitionToNewState(typeof(Connecting));
			base.OpenConnection();
			base.TransitionToNewState(typeof(Connected));

			base.Logger.Info("Connect Completed");
		}
	}
}