using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;


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
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _bus = new PMCG.Messaging.Client.Bus(_busConfirguration);
			_bus.Connect();

			_bus.PublishAsync<MyEvent>(null);
		}


		[Test]
		public void TestMockingingABus()
		{
			var _bus = Substitute.For<IBus>();

			var _event = new MyEvent(Guid.NewGuid(), "", "", 1);
			var _result = new TaskCompletionSource<PublicationResult>();
			_result.SetResult(new PublicationResult(PublicationResultStatus.Published, _event));
			
			_bus.PublishAsync(_event).Returns(_result.Task);
			_bus.PublishAsync(Arg.Any<MyEvent>()).Returns(_result.Task);
		}
	}
}
