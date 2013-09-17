﻿using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Subscribers
	{
		private int c_numberOfSubscribers = 2;
		private IConnection c_connection;
		private CancellationTokenSource c_cancellationTokenSource;
		private Task[] c_subscriberTasks;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateSubscriberTasks();
			Array.ForEach(this.c_subscriberTasks, task => task.Start());

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateSubscriberTasks();
			Array.ForEach(this.c_subscriberTasks, task => task.Start());

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Start_Then_Cancel_Token_And_Then_Close_Connection()
		{
			this.InstantiateSubscriberTasks();
			Array.ForEach(this.c_subscriberTasks, task => task.Start());

			Console.WriteLine("Hit enter to cancel the token, should terminate the subscriber, subject to the dequeue timeout");
			Console.ReadLine();
			this.c_cancellationTokenSource.Cancel();

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateSubscriberTasks()
		{
			var _logger = new ConsoleLogger();

			var _connectionUri = "amqp://guest:guest@localhost:5672/dev";
			this.c_connection = new ConnectionFactory { Uri = _connectionUri }.CreateConnection();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "......";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			this.c_cancellationTokenSource = new CancellationTokenSource();

			this.c_subscriberTasks = new Task[this.c_numberOfSubscribers];
			for(var _index = 0; _index < this.c_numberOfSubscribers; _index++)
			{
				this.c_subscriberTasks[_index] = new Task(() =>
					new PMCG.Messaging.Client.Subscriber(
						_logger,
						this.c_connection,
						_busConfigurationBuilder.Build(),
						this.c_cancellationTokenSource.Token)
						.Start());
				};
		}
	}
}