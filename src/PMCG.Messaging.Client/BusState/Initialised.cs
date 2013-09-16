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
			// Will block until the connection is open
			base.OpenConnection();
			base.TransitionToNewState(typeof(Connected));

			base.Logger.Info("Completed");
		}
	}
}