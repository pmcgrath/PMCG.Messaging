using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class ConsumerMessageProcessor
	{
		private IModel c_channel;
		private PMCG.Messaging.Client.ConsumerMessageProcessor c_SUT;
		private ConsumerHandlerResult c_messageProcessrResult;
		private CancellationTokenSource c_cancellationTokenSource;


		[SetUp]
		public void SetUp()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("....");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"d:\temp";
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				"TheQueueName",
				typeof(MyEvent).Name,
				message =>
					{
						this.c_messageProcessrResult = ConsumerHandlerResult.Completed;
						return ConsumerHandlerResult.Completed;
					});
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				"TheQueueName",
				typeof(MyEvent).FullName,
				message =>
					{
						this.c_messageProcessrResult = ConsumerHandlerResult.Completed;
						return ConsumerHandlerResult.Completed;
					});
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				"TheQueueName",
				"Throw_Error_Type_Header",
				message =>
					{
						throw new ApplicationException("Bang !");
					});
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				"TheQueueName",
				"Returns_Errored_Result",
				message =>
					{
						this.c_messageProcessrResult = ConsumerHandlerResult.Errored;
						return ConsumerHandlerResult.Errored;
					});
			var _busConfiguration = _busConfigurationBuilder.Build();

			var _connection = Substitute.For<IConnection>();

			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_channel = Substitute.For<IModel>();
			_connection.CreateModel().Returns(this.c_channel);

			this.c_SUT = new PMCG.Messaging.Client.ConsumerMessageProcessor(_busConfiguration);

			this.c_messageProcessrResult = ConsumerHandlerResult.None;
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
			Assert.AreEqual(ConsumerHandlerResult.Completed, this.c_messageProcessrResult);
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
			Assert.AreEqual(ConsumerHandlerResult.None, this.c_messageProcessrResult);
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
			Assert.AreEqual(ConsumerHandlerResult.Errored, this.c_messageProcessrResult);
		}
	}
}
