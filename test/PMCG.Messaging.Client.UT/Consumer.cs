using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Consumer
	{
		private BusConfiguration c_busConfiguration;
		private IConnection c_connection;
		private IModel c_channel;
		private CancellationTokenSource c_cancellationTokenSource;


		[SetUp]
		public void SetUp()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.ConsumerDequeueTimeout = TimeSpan.FromMilliseconds(20);
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message =>
					{
						return ConsumerHandlerResult.Completed;
					});
			this.c_busConfiguration = _busConfigurationBuilder.Build();

			this.c_connection = Substitute.For<IConnection>();
			this.c_channel = Substitute.For<IModel>();
			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_connection.CreateModel().Returns(this.c_channel);
		}


		[Test, ExpectedException(typeof(ApplicationException))]
		public void Start_Where_Cancellation_Token_Already_Canceled_Results_In_an_Exception()
		{
			this.c_cancellationTokenSource.Cancel();
			var _SUT = new PMCG.Messaging.Client.Consumer(
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			_SUT.Start();
		}


		[Test]
		public void Cancellation_Token_Cancelled_Where_Already_Started_Results_In_Consumer_Completion()
		{
			this.c_channel.IsOpen.Returns(true);

			var _SUT = new PMCG.Messaging.Client.Consumer(
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			var _consumerTask = _SUT.Start();

			Thread.Sleep(40);	// Allow task to start
			this.c_cancellationTokenSource.Cancel();
			Thread.Sleep(30);	// Allow queue dequeue call to timeout

			Assert.IsTrue(_SUT.IsCompleted);
			this.c_channel.Received().Close();
		}


		[Test]
		public void Consume_Where_We_Mock_All_Without_A_Real_Connection_Knows_Too_Much_About_RabbitMQ_Internals()
		{
			var _capturedMessageId = Guid.Empty;

			var _configurationBuilder = new BusConfigurationBuilder();
			_configurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_configurationBuilder.RegisterConsumer<MyEvent>(
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message =>
					{
						_capturedMessageId = message.Id;
						return ConsumerHandlerResult.Completed;
					});
			var _configuration = _configurationBuilder.Build();

			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			
			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
	
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageProperties = Substitute.For<IBasicProperties>();
			_messageProperties.ContentType = "application/json";
			_messageProperties.DeliveryMode = (byte)MessageDeliveryMode.Persistent;
			_messageProperties.Type = typeof(MyEvent).Name;
			_messageProperties.MessageId = _myEvent.Id.ToString();
			_messageProperties.CorrelationId = _myEvent.CorrelationId;
			_channel.CreateBasicProperties().Returns(_messageProperties);

			QueueingBasicConsumer _capturedConsumer = null;
			_channel
				.When(channel => channel.BasicConsume(TestingConfiguration.QueueName, false, Arg.Any<IBasicConsumer>()))
				.Do(callInfo => _capturedConsumer = callInfo[2] as QueueingBasicConsumer);

			var _SUT = new PMCG.Messaging.Client.Consumer(_connection, _configuration, CancellationToken.None);
			var _consumerTask = _SUT.Start();
			// Time for other consumer to start on other thread, we need to wait for start to complete before we publish
			Thread.Sleep(50);		// To get to work on first run, needs to be much higher (5000), but this value will work for subsequent calls

			var _messageJson = JsonConvert.SerializeObject(_myEvent);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);
			_capturedConsumer.Queue.Enqueue(
				new BasicDeliverEventArgs
					{
						ConsumerTag = "consumerTag",
						DeliveryTag = 1UL,
						Redelivered = false,
						Exchange = "TheExchange",
						RoutingKey = "ARoutingKey",
						BasicProperties = _messageProperties,
						Body = _messageBody
					});
			// Time for delivery on the other thread and the consumer callback to complete
			Thread.Sleep(500);

			Assert.AreEqual(_myEvent.Id, _capturedMessageId);
		}
	}
}
