using Common.Logging;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.Client
{
	public static class ServiceLocator
	{
		public static Func<ILog> GetLogger =
			() => new Common.Logging.Simple.ConsoleOutLogger("App", LogLevel.All, true, true, false, "hh:mm:ss");

		public static Func<ILog, BusConfiguration, IConnectionManager> GetConnectionManager =
			(logger, busConfiguration) => new ConnectionManager(logger, busConfiguration.ConnectionUri, busConfiguration.ReconnectionPauseInterval);


		public static Func<BusConfiguration, IStore> GetNewDisconnectedStore = 
			(busConfiguration) => new FileSystemStore(busConfiguration.DisconnectedMessagesStoragePath);
	}
}
