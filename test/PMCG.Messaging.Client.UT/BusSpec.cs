using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class BusSpec
	{
		[Test]
		public void Status_Initialised()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _SUT = new Bus(_busConfirguration);

			Assert.AreEqual(BusStatus.Initialised, _SUT.Status);
		}


		[Test]
		[Category("RequiresRunningInstance")]
		public void Status_Connected_And_Then_Closed()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _SUT = new Bus(_busConfirguration);

			_SUT.Connect();
			Assert.AreEqual(BusStatus.Connected, _SUT.Status);

			_SUT.Close();
			Assert.AreEqual(BusStatus.Closed, _SUT.Status);
		}

	
		[Test, ExpectedException(typeof(ArgumentNullException))]
		[Category("RequiresRunningInstance")]
		public void PublishAsync_Null_Message_Results_In_An_Exception()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _SUT = new Bus(_busConfirguration);
			_SUT.Connect();

			_SUT.PublishAsync<MyEvent>(null);
		}


		[Test]
		public void TestMockingingABus()
		{
			var _bus = Substitute.For<IBus>();

			var _event = new MyEvent(Guid.NewGuid(), "", "", 1);
			var _result = new TaskCompletionSource<PMCG.Messaging.PublicationResult>();
			_result.SetResult(new PMCG.Messaging.PublicationResult(PMCG.Messaging.PublicationResultStatus.Published, _event));
			
			_bus.PublishAsync(_event).Returns(_result.Task);
			_bus.PublishAsync(Arg.Any<MyEvent>()).Returns(_result.Task);
		}
	}
}
