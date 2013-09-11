using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.RabbitMQ.UT
{
	[TestFixture]
	public class Subscriber
	{
		private MessageSubscriptions c_messageSubscriptionConfigurations;
		private IModel c_channel;
		private PMCG.Messaging.RabbitMQ.Subscriber c_SUT;
		private MessageSubscriptionActionResult c_messageHandlerResult;


		[SetUp]
		public void SetUp()
		{
			this.c_messageSubscriptionConfigurations = new MessageSubscriptions(
				1,
				new[]
					{
						new MessageSubscription(
							typeof(AnEvent),
							"TheQueueName",
							typeof(AnEvent).Name,
							message =>
								{
									this.c_messageHandlerResult = MessageSubscriptionActionResult.Completed;
									return MessageSubscriptionActionResult.Completed;
								}),
						new MessageSubscription(
							typeof(AnEvent),
							"TheQueueName",
							typeof(AnEvent).FullName,
							message =>
								{
									this.c_messageHandlerResult = MessageSubscriptionActionResult.Completed;
									return MessageSubscriptionActionResult.Completed;
								}),
						new MessageSubscription(
							typeof(AnEvent),
							"TheQueueName",
							"Throw_Error_Type_Header",
							message =>
								{
									throw new ApplicationException("Bang !");
								}),
						new MessageSubscription(
							typeof(AnEvent),
							"TheQueueName",
							"Returns_Errored_Result",
							message =>
								{
									this.c_messageHandlerResult = MessageSubscriptionActionResult.Errored;
									return MessageSubscriptionActionResult.Errored;
								})
					});

			var _logger = Substitute.For<ILog>();
			var _connection = Substitute.For<IConnection>();

			this.c_channel = Substitute.For<IModel>();
			_connection.CreateModel().Returns(this.c_channel);

			this.c_SUT = new PMCG.Messaging.RabbitMQ.Subscriber(
				_logger,
				_connection,
				this.c_messageSubscriptionConfigurations);
			this.c_SUT.Start();

			this.c_messageHandlerResult = MessageSubscriptionActionResult.None;
		}


		[Test]
		public void Handle_Where_Unknown_Type_Results_In_Channel_Being_Acked()
		{
			var _subscriptionMessage = new SubscriptionMessage(
				body: new byte[0],
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				exchange: "TheExchangeName",
				redelivered: false,
				routingKey: "RoutingKey",
				type: "Unknown");

			this.c_SUT.Handle(_subscriptionMessage);

			this.c_channel.Received().BasicNack(_subscriptionMessage.DeliveryTag, false, false);
		}

	
		[Test]
		public void Handle_Where_Known_Type_Results_In_Channel_Being_Acked()
		{
			var _subscriptionMessage = new SubscriptionMessage(
				body: new byte[0],
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				exchange: "TheExchangeName",
				redelivered: false,
				routingKey: "RoutingKey",
				type: typeof(AnEvent).Name);

			this.c_SUT.Handle(_subscriptionMessage);

			this.c_channel.Received().BasicAck(_subscriptionMessage.DeliveryTag, false);
			Assert.AreEqual(MessageSubscriptionActionResult.Completed, this.c_messageHandlerResult);
		}


		[Test]
		public void Handle_Where_Message_Action_Throws_Exception_Results_In_Channel_Being_Nacked()
		{
			var _subscriptionMessage = new SubscriptionMessage(
				body: new byte[0],
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				exchange: "TheExchangeName",
				redelivered: false,
				routingKey: "RoutingKey",
				type: "Throw_Error_Type_Header");

			this.c_SUT.Handle(_subscriptionMessage);

			this.c_channel.Received().BasicNack(_subscriptionMessage.DeliveryTag, false, false);
			Assert.AreEqual(MessageSubscriptionActionResult.None, this.c_messageHandlerResult);
		}


		[Test]
		public void Handle_Where_Message_Action_Returns_Errored_Results_In_Channel_Being_Nacked()
		{
			var _subscriptionMessage = new SubscriptionMessage(
				body: new byte[0],
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				exchange: "TheExchangeName",
				redelivered: false,
				routingKey: "RoutingKey",
				type: "Returns_Errored_Result");

			this.c_SUT.Handle(_subscriptionMessage);

			this.c_channel.Received().BasicNack(_subscriptionMessage.DeliveryTag, false, false);
			Assert.AreEqual(MessageSubscriptionActionResult.Errored, this.c_messageHandlerResult);
		}
	}
}
