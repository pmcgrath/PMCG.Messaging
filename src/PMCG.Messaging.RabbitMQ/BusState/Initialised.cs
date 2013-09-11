using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.RabbitMQ.BusState
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