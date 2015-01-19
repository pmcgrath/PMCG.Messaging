using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.BusState;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;


namespace PMCG.Messaging.Client.UT.BusState
{
	[TestFixture]
	public class ClosedSpec
	{
		[Test]
		public void Ctor_Success()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Closed(
				_busConfirguration,
				_connectionManager,
				_context);

			Assert.IsNotNull(_SUT);
		}


		[Test]
		public void Connect_Where_Connection_Is_Established_Results_In_Transition_To_Connected_State()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Closed(
				_busConfirguration,
				_connectionManager,
				_context);

			_context.State.Returns(callInfo => _SUT);
			List<State> _capturedStates = new List<State>();
			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedStates.Add(callInfo[0] as State));

			_SUT.Connect();

			Assert.IsInstanceOf<Connecting>(_capturedStates[0]);
			Assert.IsInstanceOf<Connected>(_capturedStates[1]);
		}


		[Test, Ignore("Pending completion - The ConnectionManager.Open call is a blocking call")]
		public void Connect_Where_Connection_Is_Not_Established_Results_In_Transition_To_Connecting_State()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Closed(
				_busConfirguration,
				_connectionManager,
				_context);

			_context.State.Returns(callInfo => _SUT);
			State _capturedState = null;
			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);

			_SUT.Connect();

			Assert.IsInstanceOf<Connecting>(_capturedState);
		}


		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void Close_Results_In_An_Invalid_Operation_Exception()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Closed(
				_busConfirguration,
				_connectionManager,
				_context);

			_SUT.Close();
		}


		[Test, ExpectedException()]
		public void Publish_Results_In_An_Exception()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new Closed(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			_SUT.PublishAsync(_theEvent);
		}
	}
}
