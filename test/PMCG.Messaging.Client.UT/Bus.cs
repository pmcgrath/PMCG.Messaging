using NUnit.Framework;
using System;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Bus
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void PublishAsync_Null_Message_Results_In_An_Exception()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _bus = new PMCG.Messaging.Client.Bus(_busConfirguration);
			_bus.Connect();

			_bus.PublishAsync<MyEvent>(null);
		}
	}
}
