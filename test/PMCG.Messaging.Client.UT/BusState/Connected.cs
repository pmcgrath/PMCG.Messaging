using PMCG.Messaging.Client.BusState;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
			_channel.NextPublishSeqNo.Returns(1UL, 2UL, 3UL, 4UL);
			
			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
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
			_channel.NextPublishSeqNo.Returns(1UL, 2UL);

			var _SUT = new PMCG.Messaging.Client.BusState.Connected(
				_busConfirguration,
				_connectionManager,
				_context);

			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
			var _publicationResult = _SUT.PublishAsync(_theEvent);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
			_channel.BasicNacks += Raise.Event<BasicNackEventHandler>(_channel, new BasicNackEventArgs { Multiple = false, DeliveryTag = 2 });
			_publicationResult.Wait();

			Assert.AreEqual(PublicationResultStatus.NotPublished, _publicationResult.Result.Status);
		}




		[Test]
		public void T()
		{
			var _channel = Substitute.For<IModel>();
			_channel.NextPublishSeqNo.Returns(1UL, 2UL);

			var _1 = _channel.NextPublishSeqNo;
			var _2 = _channel.NextPublishSeqNo;
			var _3 = _channel.NextPublishSeqNo;

		}
	}
}
