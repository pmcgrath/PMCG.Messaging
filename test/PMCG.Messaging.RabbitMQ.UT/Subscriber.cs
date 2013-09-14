using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.RabbitMQ.UT
{
	[TestFixture]
	public class Subscriber
	{
		private BusConfiguration c_busConfiguration;
		private ILog c_logger;
		private IConnection c_connection;
		private IModel c_channel;
		private CancellationTokenSource c_cancellationTokenSource;


		[SetUp]
		public void SetUp()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "....";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"d:\temp";
			_busConfigurationBuilder.SubscriptionDequeueTimeout = TimeSpan.FromMilliseconds(20);
			_busConfigurationBuilder.RegisterSubscription<AnEvent>(
				"TheQueueName",
				typeof(AnEvent).Name,
				message =>
				{
					return MessageSubscriptionActionResult.Completed;
				});
			this.c_busConfiguration = _busConfigurationBuilder.Build();

			this.c_logger = Substitute.For<ILog>();
			this.c_connection = Substitute.For<IConnection>();
			this.c_channel = Substitute.For<IModel>();
			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_connection.CreateModel().Returns(this.c_channel);
		}


		[Test, ExpectedException(typeof(ApplicationException))]
		public void Start_Where_Cancellation_Token_Already_Canceled_Results_In_an_Exception()
		{
			this.c_cancellationTokenSource.Cancel();
			var _SUT = new PMCG.Messaging.RabbitMQ.Subscriber(
				this.c_logger,
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			_SUT.Start();
		}


		[Test]
		public void Cancellation_Token_Cancelled_Where_Already_Started_Results_In_Subscriber_Completion()
		{
			this.c_channel.IsOpen.Returns(true);

			var _SUT = new PMCG.Messaging.RabbitMQ.Subscriber(
				this.c_logger,
				this.c_connection,
				this.c_busConfiguration,
				this.c_cancellationTokenSource.Token);
			new Task(_SUT.Start).Start();

			Thread.Sleep(40);	// Allow task to start
			this.c_cancellationTokenSource.Cancel();
			Thread.Sleep(30);	// Allow queue dequeue call to timeout

			Assert.IsTrue(_SUT.IsCompleted);
			this.c_channel.Received().Close();
		}
	}
}
