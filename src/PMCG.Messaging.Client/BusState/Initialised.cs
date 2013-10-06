using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.Client.BusState
{
	public class Initialised : State
	{
		public Initialised(
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(logger, configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info();
		}


		public override void Connect()
		{
			base.Logger.Info();
			
			base.TransitionToNewState(typeof(Connecting));
			base.OpenConnectionInitially();
			base.TransitionToNewState(base.ConnectionManager.IsOpen ? typeof(Connected) : typeof(Disconnected));

			base.Logger.Info("Completed");
		}
	}
}