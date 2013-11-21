using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Consumer
	{
		private IConnection c_connection;
		private CancellationTokenSource c_cancellationTokenSource;
		private PMCG.Messaging.Client.Consumer c_consumer;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateConsumer();
			new Task(this.c_consumer.Start).Start();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateConsumer();
			new Task(this.c_consumer.Start).Start();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Create_A_Transient_Queue_And_Then_Close_Connection()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = Configuration.DisconnectedMessagesStoragePath;
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(
					Configuration.ExchangeName1,
					typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; },
				Configuration.ExchangeName1);
			this.InstantiateConsumer(_busConfigurationBuilder.Build());
			new Task(this.c_consumer.Start).Start();

			Console.WriteLine("You should see a new transient queue in the dashboard)");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateConsumer()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = Configuration.DisconnectedMessagesStoragePath;

			this.InstantiateConsumer(_busConfigurationBuilder.Build());
		}


		public void InstantiateConsumer(
			BusConfiguration busConfiguration)
		{
			this.c_connection = new ConnectionFactory { Uri = busConfiguration.ConnectionUris.First() }.CreateConnection();
			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_consumer = new PMCG.Messaging.Client.Consumer(
				this.c_connection,
				busConfiguration,
				this.c_cancellationTokenSource.Token);
		}
	}
}
