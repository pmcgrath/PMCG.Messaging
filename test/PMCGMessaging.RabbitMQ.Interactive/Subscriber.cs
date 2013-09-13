using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	public class Subscriber
	{
		private IConnection c_connection;
		private CancellationTokenSource c_cancellationTokenSource;
		private PMCG.Messaging.RabbitMQ.Subscriber c_subscriber;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateSubscriber();
			new Task(this.c_subscriber.Start).Start();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateSubscriber();
			new Task(this.c_subscriber.Start).Start();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After clsoing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Start_Then_Cancel_Token_And_Then_Close_Connection()
		{
			this.InstantiateSubscriber();
			new Task(this.c_subscriber.Start).Start();

			Console.WriteLine("Hit enter to cancel the token, should terminate the subscriber, subject to the dequeue timeout");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateSubscriber()
		{
			var _logger = new Log();

			var _connectionUri = "amqp://guest:guest@localhost:5672/dev";
			this.c_connection = new ConnectionFactory { Uri = _connectionUri }.CreateConnection();

			var _busConfiguration = ConfigurationFactory.CreateABusConfiguration();

			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_subscriber = new PMCG.Messaging.RabbitMQ.Subscriber(
				_logger,
				this.c_connection,
 				_busConfiguration,
 				this.c_cancellationTokenSource.Token);
		}
	}
}
