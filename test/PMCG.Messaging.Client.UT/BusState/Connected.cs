using NUnit.Framework;
using System;


namespace PMCG.Messaging.Client.UT.BusState
{
	[TestFixture]
	public class Connected
	{
		[Test]
		public void Ctor_Success()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					typeof(MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; },
					TestingConfiguration.ExchangeName);
			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connection = Substitute.For<IConnection>();
//			var _connnectionManager = new PMCG.Messaging.Client.ConnectionManager(   
//			_channel = Substitute.For<IModel>();
//			this.c_cancellationTokenSource = new CancellationTokenSource();

//			this.c_connection.CreateModel().Returns(this.c_channel);



			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				null,
				null);
		}
	}
}
