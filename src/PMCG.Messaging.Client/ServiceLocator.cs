using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client
{
	public static class ServiceLocator
	{
		public static Func<BusConfiguration, IConnectionManager> GetConnectionManager =
			busConfiguration => new ConnectionManager(busConfiguration.ConnectionUris, busConfiguration.HeartbeatInterval, busConfiguration.ReconnectionPauseInterval);
	}
}
