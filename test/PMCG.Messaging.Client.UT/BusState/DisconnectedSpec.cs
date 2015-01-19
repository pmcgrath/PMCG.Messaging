using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.BusState;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading;


namespace PMCG.Messaging.Client.UT.BusState
{
	[TestFixture]
	public class DisconnectedSpec
	{
		[Test]
		public void Ctor_Success()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			Assert.IsNotNull(_SUT);
		}


		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void Connect_Results_In_An_Invalid_Operation_Exception()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			_SUT.Connect();
		}


		[Test]
		public void Publish_Where_No_Publication_Configurations_Which_Results_In_A_NoConfigurationFound_Result()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_publicationResult.Wait();

			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NoConfigurationFound, _publicationResult.Result.Status);
		}

	
		[Test]
		public void Publish_Where_Publication_Configurations_Exist_Which_Results_In_Disconnected_Result()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_publicationResult.Wait();

			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.Disconnected, _publicationResult.Result.Status);
		}


		[Test]
		public void Close_Results_In_Transition_To_Closed_State()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			_context.State.Returns(callInfo => _SUT);
			State _capturedState = null;
			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);
			_SUT.Close();

			Assert.IsInstanceOf<Closed>(_capturedState);
			_connectionManager.Received().Close();
		}


		[Test]
		public void State_Changed_Where_Connection_Is_Established_Results_In_Transition_To_Connected_State()
		{
			var _connectionEstablishedWaitHandle = new AutoResetEvent(false);
			var _transitionBeingAttemptedWaitHandle = new AutoResetEvent(false);

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			_connectionManager.IsOpen.Returns(true);
			_connectionManager
				.When(connectionManager => connectionManager.Open())
				.Do(callInfo => { _connectionEstablishedWaitHandle.WaitOne(); });

			var _SUT = new Disconnected(
				_busConfirguration,
				_connectionManager,
				_context);

			_context.State.Returns(callInfo => _SUT);
			State _capturedState = null;
			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo =>
				{
					_capturedState = callInfo[0] as State;
					_transitionBeingAttemptedWaitHandle.Set();
				});
			_connectionEstablishedWaitHandle.Set();				// Allow ConnectionManagers Open method to complete
			_transitionBeingAttemptedWaitHandle.WaitOne();		// Wait on state transition completion

			Assert.IsInstanceOf<Connected>(_capturedState);
		}
	}
}
