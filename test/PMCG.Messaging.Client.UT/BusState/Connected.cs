using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.BusState;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;


namespace PMCG.Messaging.Client.UT.BusState
{
	// The tests with multiple publications are not quite right as we are using a single channel for all the publications, the internal 
	// Parallel.ForEach call will result in the two publications being run on sperate tasks and probably seperate threads, could have
	// created multiple channels and passed to the Returns method for CreateModel, would need to use different NextPublishSeqNo values
	// and would need to invoke BasicAsk events on both channels, as the ToString() method on the channel returns the same value which 
	// means they would all have the same unconfirmed publisher dictionary entry key and therefore some tasks could never complete
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
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);
		}


		[Test]
		public void Publish_Where_No_Publication_Configurations_Which_Results_In_A_NoConfigurationFound_Result()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _context = Substitute.For<IBusContext>();

			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_publicationResult.Wait();

			Assert.AreEqual(PublicationResultStatus.NoConfigurationFound, _publicationResult.Result.Status);
		}

	
		[Test]
		public void Publish_Where_A_Single_Publication_Configurations_Which_Results_In_Successfull_Publication()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _context = Substitute.For<IBusContext>();

			_connectionManager.Connection.Returns(_connection);
			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
			_channel.NextPublishSeqNo.Returns(1UL);

			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 10 });
			_publicationResult.Wait();

			Assert.AreEqual(PublicationResultStatus.Published, _publicationResult.Result.Status);
		}


		[Test]
		public void Publish_Where_Multiple_Publication_Configurations_Which_Results_In_Successfull_Publication()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _context = Substitute.For<IBusContext>();

			_connectionManager.Connection.Returns(_connection);
			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
			_channel.NextPublishSeqNo.Returns(1UL, 2UL);		// Not sure why it works here, see next method comment
			
			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 2 });
			_publicationResult.Wait();

			Assert.AreEqual(PublicationResultStatus.Published, _publicationResult.Result.Status);
		}


		[Test]
		public void Publish_Where_Multiple_Publication_Configurations_One_Of_Which_Is_Nacked_Results_In_Unsuccessfull_Publication()
		{
			var _busConfigurationBuilder = new PMCG.Messaging.Client.Configuration.BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = TestingConfiguration.DisconnectedMessagesStoragePath;
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>(
					TestingConfiguration.ExchangeName,
					typeof(MyEvent).Name);
			var _busConfirguration = _busConfigurationBuilder.Build();

			var _connectionManager = Substitute.For<IConnectionManager>();
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _context = Substitute.For<IBusContext>();

			_connectionManager.Connection.Returns(_connection);
			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
			var _nextPublishSeqNo = 1UL;
			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !

			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 2 });
			_channel.BasicNacks += Raise.Event<BasicNackEventHandler>(_channel, new BasicNackEventArgs { Multiple = false, DeliveryTag = 3 });
			_publicationResult.Wait();

			Assert.AreEqual(PublicationResultStatus.NotPublished, _publicationResult.Result.Status);
		}
	}
}
