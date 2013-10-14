using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Consumers
	{
		private int c_numberOfConsumers = 2;
		private IConnection c_connection;
		private CancellationTokenSource c_cancellationTokenSource;
		private Task[] c_consumerTasks;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateConsumerTasks();
			Array.ForEach(this.c_consumerTasks, task => task.Start());

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateConsumerTasks();
			Array.ForEach(this.c_consumerTasks, task => task.Start());

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Start_Then_Cancel_Token_And_Then_Close_Connection()
		{
			this.InstantiateConsumerTasks();
			Array.ForEach(this.c_consumerTasks, task => task.Start());

			Console.WriteLine("Hit enter to cancel the token, should terminate the consumer, subject to the dequeue timeout");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateConsumerTasks()
		{
			var _connectionUri = "amqp://guest:guest@localhost:5672/";
			this.c_connection = new ConnectionFactory { Uri = _connectionUri }.CreateConnection();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(_connectionUri);
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_consumerTasks = new Task[this.c_numberOfConsumers];
			for(var _index = 0; _index < this.c_numberOfConsumers; _index++)
			{
				this.c_consumerTasks[_index] = new Task(() =>
					new PMCG.Messaging.Client.Consumer(
						this.c_connection,
						_busConfigurationBuilder.Build(),
						this.c_cancellationTokenSource.Token)
						.Start());
				};
		}
	}
}
