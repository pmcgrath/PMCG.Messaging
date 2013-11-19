using Common.Logging;
using PMCG.Messaging.Client.BusState;
using PMCG.Messaging.Client.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Bus : IBusContext, IBusController, IBus
	{
		private readonly ILog c_logger;


		public State State { get; set; }


		public Bus(
			BusConfiguration configuration)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

			Check.RequireArgumentNotNull("configuration", configuration);
			Check.RequireArgument("configuration.DisconnectedMessagesStoragePath", configuration.DisconnectedMessagesStoragePath,
				Directory.Exists(configuration.DisconnectedMessagesStoragePath));

			var _connectionManager = ServiceLocator.GetConnectionManager(configuration);

			this.State = new Initialised(
				configuration,
				_connectionManager,
				this);

			this.c_logger.Info("ctor Completed");
		}


		public void Connect()
		{
			this.c_logger.Info("Connect Starting");
			this.State.Connect();
			this.c_logger.Info("Connect Completed");
		}


		public void Close()
		{
			this.c_logger.Info("Close Starting");
			this.State.Close();
			this.c_logger.Info("Close Completed");
		}


		public Task PublishAsync<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			this.c_logger.Info("PublishAsync Starting");
			Check.RequireArgumentNotNull("message", message);

			var _result = this.State.PublishAsync(message);

			this.c_logger.Info("PublishAsync Completed");
			return _result;
		}
	}
}