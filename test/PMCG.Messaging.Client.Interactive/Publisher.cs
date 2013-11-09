using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Threading;


namespace PMCG.Messaging.Client.Interactive
{
	public class Publisher
	{
		private IConnection c_connection;
		private TimeSpan c_publicationTimeout;
		private CancellationTokenSource c_cancellationTokenSource;
		private PMCG.Messaging.Client.Publisher c_publisher;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiatePublisher();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiatePublisher();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_Messages()
		{
			this.InstantiatePublisher();

			do
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "", "DDD....", 1);
				this.c_publisher.Publish(
					new QueuedMessage(
						new MessageDelivery("test.exchange.1", "H", MessageDeliveryMode.Persistent, m => "ted"), _myEvent));
				Console.WriteLine("Hit enter to publish another message, x to exit");
			} while (Console.ReadLine() != "x");

			Console.WriteLine("Hit enter to cancel");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_Async_Messages()
		{
			this.InstantiatePublisher();

			do
			{
				for (var _index = 1; _index <= 100; _index++)
				{
					var _myEvent = new MyEvent(Guid.NewGuid(), "", "DDD....", _index);
					this.c_publisher.PublishAsync(
						new QueuedMessage(
							new MessageDelivery("test.exchange.1", "H", MessageDeliveryMode.Persistent, m => "Ted"), _myEvent));
				}
				Console.WriteLine("Hit enter to publish more messages, x to exit");
			} while (Console.ReadLine() != "x");

			Console.WriteLine("Hit enter to cancel");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_Async_A_Message_To_A_Non_Existent_Exchange_Will_Close_The_Internal_Channel()
		{
			this.InstantiatePublisher();

			Console.WriteLine("Hit enter to publish async");
			Console.ReadLine();
			var _myEvent = new MyEvent(Guid.NewGuid(), "", "DDD....", 1);

			var _task = this.c_publisher.PublishAsync(
				new QueuedMessage(
					new MessageDelivery("NON_EXISTENT_EXCHANGE", "H", MessageDeliveryMode.Persistent, m => "Ted"), _myEvent));
			try
			{
				_task.Wait();
			}
			catch (AggregateException exception)
			{
				Console.WriteLine("Exception - should be 404 channel sutdown - {0}", exception.InnerExceptions[0].Message);
			}

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiatePublisher()
		{
			this.c_connection = new ConnectionFactory { Uri = "amqp://guest:guest@localhost:5672/" }.CreateConnection();
			this.c_publicationTimeout = TimeSpan.FromMilliseconds(500);
			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_publisher = new PMCG.Messaging.Client.Publisher(
				this.c_connection,
				this.c_publicationTimeout,
				this.c_cancellationTokenSource.Token);
		}
	}
}
