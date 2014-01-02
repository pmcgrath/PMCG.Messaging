using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class ConsumerSpec
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
			var _SUT = new Consumer(
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			_SUT.Start();
		}


		[Test]
		public void Cancellation_Token_Cancelled_Where_Already_Started_Results_In_Consumer_Completion()
		{
			this.c_channel.IsOpen.Returns(true);

			var _SUT = new Consumer(
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			var _consumerTask = _SUT.Start();
			while (_consumerTask.Status != TaskStatus.Running) { } // Spin till task starts

			this.c_cancellationTokenSource.Cancel();
			Thread.Sleep(30);	// Allow dequeue call to timeout, see above where we configured to be very short

			Assert.IsTrue(_SUT.IsCompleted);
			this.c_channel.Received().Close();
		}


		[Test]
		public void Consume_Where_We_Mock_All_Without_A_Real_Connection_Knows_Too_Much_About_RabbitMQ_Internals()
		{
			var _waitHandle = new AutoResetEvent(false);
			var _capturedMessageId = Guid.Empty;

			var _configurationBuilder = new BusConfigurationBuilder();
			_configurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_configurationBuilder.RegisterConsumer<MyEvent>(
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message =>
					{
						_capturedMessageId = message.Id;
						_waitHandle.Set();
						return ConsumerHandlerResult.Completed;
					});
			var _configuration = _configurationBuilder.Build();

			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			
			_connection.CreateModel().Returns(_channel);
	
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
				.Do(callInfo => { _capturedConsumer = callInfo[2] as QueueingBasicConsumer; _waitHandle.Set(); });

			var _SUT = new Consumer(_connection, _configuration, CancellationToken.None);
			var _consumerTask = _SUT.Start();
			_waitHandle.WaitOne();			// Wait till consumer task has called the BasicConsume method which captures the consumer
			_waitHandle.Reset();			// Reset so we can block on the consumer message func

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
			_waitHandle.WaitOne();		// Wait for message to be consumed

			Assert.AreEqual(_myEvent.Id, _capturedMessageId);
		}
	}
}
