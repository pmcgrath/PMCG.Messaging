using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.Client.BusState
{
	public class Connecting : State
	{
		public Connecting(
			ILog logger,
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<QueuedMessage> queuedMessages,
			IBusContext context)
			: base(logger, configuration, connectionManager, queuedMessages, context)
		{
			base.Logger.Info();
		}


		public override void Close()
		{
			// Pending - with the blocking open connection will we get here - this is just a transient state
			base.Logger.Info();

			base.CloseConnection();
			base.TransitionToNewState(typeof(Closed));

			base.Logger.Info("Completed");
		}
	}
}