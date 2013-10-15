using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.DisconnectedStorage;
using System;


namespace PMCG.Messaging.Client
{
	public static class ServiceLocator
	{
		public static Func<BusConfiguration, IConnectionManager> GetConnectionManager =
			busConfiguration => new ConnectionManager(busConfiguration.ConnectionUris, busConfiguration.ReconnectionPauseInterval);


		public static Func<BusConfiguration, IStore> GetNewDisconnectedStore = 
			busConfiguration => new FileSystemStore(busConfiguration.DisconnectedMessagesStoragePath);
	}
}
