using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
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
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"d:\temp";
			_busConfigurationBuilder.ConsumerDequeueTimeout = TimeSpan.FromMilliseconds(20);
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				"TheQueueName",
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
			new Task(_SUT.Start).Start();

			Thread.Sleep(40);	// Allow task to start
			this.c_cancellationTokenSource.Cancel();
			Thread.Sleep(30);	// Allow queue dequeue call to timeout

			Assert.IsTrue(_SUT.IsCompleted);
			this.c_channel.Received().Close();
		}
	}
}
