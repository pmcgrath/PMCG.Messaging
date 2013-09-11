using PMCG.Messaging.RabbitMQ.BusState;
using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Concurrent;
using System.IO;


namespace PMCG.Messaging.RabbitMQ
{
	public class Bus : IBusContext, IBusController, IBus
	{
		private readonly ILog c_logger;


		public State State { get; set; }


		public Bus(
			ILog logger,
			BusConfiguration configuration)
		{
			Check.RequireArgumentNotNull("logger", logger);
			Check.RequireArgumentNotNull("configuration", configuration);
			Check.RequireArgument("configuration.DisconnectedMessagesStoragePath", configuration.DisconnectedMessagesStoragePath,
				Directory.Exists(configuration.DisconnectedMessagesStoragePath));

			this.c_logger = logger;
			this.c_logger.Info();

			var _connectionManager = new ConnectionManager(
				this.c_logger,
				configuration.ConnectionUri,
				configuration.ReconnectionPauseInterval);

			this.State = new Initialised(
				this.c_logger,
				configuration,
				_connectionManager,
				new BlockingCollection<QueuedMessage>(),
				this);

			this.c_logger.Info("Completed");
		}


		public void Connect()
		{
			this.c_logger.Info();
			this.State.Connect();
			this.c_logger.Info("Completed");
		}


		public void Close()
		{
			this.c_logger.Info();
			this.State.Close();
			this.c_logger.Info("Completed");
		}


		public void Publish<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			Check.RequireArgumentNotNull("message", message);

			this.c_logger.Info();
			this.State.Publish(message);

			this.c_logger.Info("Completed");
		}
	}
}