using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class SubscriptionMessageProcessor
	{
		private IModel c_channel;
		private PMCG.Messaging.Client.SubscriptionMessageProcessor c_SUT;
		private SubscriptionHandlerResult c_messageProcessrResult;
		private CancellationTokenSource c_cancellationTokenSource;


		[SetUp]
		public void SetUp()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "....";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"d:\temp";
			_busConfigurationBuilder.RegisterSubscription<MyEvent>(
				"TheQueueName",
				typeof(MyEvent).Name,
				message =>
					{
						this.c_messageProcessrResult = SubscriptionHandlerResult.Completed;
						return SubscriptionHandlerResult.Completed;
					});
			_busConfigurationBuilder.RegisterSubscription<MyEvent>(
				"TheQueueName",
				typeof(MyEvent).FullName,
				message =>
					{
						this.c_messageProcessrResult = SubscriptionHandlerResult.Completed;
						return SubscriptionHandlerResult.Completed;
					});
			_busConfigurationBuilder.RegisterSubscription<MyEvent>(
				"TheQueueName",
				"Throw_Error_Type_Header",
				message =>
					{
						throw new ApplicationException("Bang !");
					});
			_busConfigurationBuilder.RegisterSubscription<MyEvent>(
				"TheQueueName",
				"Returns_Errored_Result",
				message =>
					{
						this.c_messageProcessrResult = SubscriptionHandlerResult.Errored;
						return SubscriptionHandlerResult.Errored;
					});
			var _busConfiguration = _busConfigurationBuilder.Build();

			var _logger = Substitute.For<ILog>();
			var _connection = Substitute.For<IConnection>();

			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_channel = Substitute.For<IModel>();
			_connection.CreateModel().Returns(this.c_channel);

			this.c_SUT = new PMCG.Messaging.Client.SubscriptionMessageProcessor(_logger, _busConfiguration);

			this.c_messageProcessrResult = SubscriptionHandlerResult.None;
		}


		[Test]
		public void Process_Where_Unknown_Type_Results_In_Channel_Being_Acked()
		{
			var _properties = Substitute.For<IBasicProperties>();
			_properties.Type.Returns("Unknown");

			var _message = new BasicDeliverEventArgs(
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				redelivered: false,
				exchange: "TheExchangeName",
				routingKey: "RoutingKey",
				properties: _properties,
				body: new byte[0]);

			this.c_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
		}

	
		[Test]
		public void Process_Where_Known_Type_Results_In_Channel_Being_Acked()
		{
			var _properties = Substitute.For<IBasicProperties>();
			_properties.Type.Returns(typeof(MyEvent).Name);

			var _message = new BasicDeliverEventArgs(
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				redelivered: false,
				exchange: "TheExchangeName",
				routingKey: "RoutingKey",
				properties: _properties,
				body: new byte[0]);

			this.c_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicAck(_message.DeliveryTag, false);
			Assert.AreEqual(SubscriptionHandlerResult.Completed, this.c_messageProcessrResult);
		}


		[Test]
		public void Process_Where_Message_Action_Throws_Exception_Results_In_Channel_Being_Nacked()
		{
			var _properties = Substitute.For<IBasicProperties>();
			_properties.Type.Returns("Throw_Error_Type_Header");

			var _message = new BasicDeliverEventArgs(
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				redelivered: false,
				exchange: "TheExchangeName",
				routingKey: "RoutingKey",
				properties: _properties,
				body: new byte[0]);

			this.c_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
			Assert.AreEqual(SubscriptionHandlerResult.None, this.c_messageProcessrResult);
		}


		[Test]
		public void Process_Where_Message_Action_Returns_Errored_Results_In_Channel_Being_Nacked()
		{
			var _properties = Substitute.For<IBasicProperties>();
			_properties.Type.Returns("Returns_Errored_Result");

			var _message = new BasicDeliverEventArgs(
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				redelivered: false,
				exchange: "TheExchangeName",
				routingKey: "RoutingKey",
				properties: _properties,
				body: new byte[0]);

			this.c_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
			Assert.AreEqual(SubscriptionHandlerResult.Errored, this.c_messageProcessrResult);
		}
	}
}
