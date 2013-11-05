using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;


namespace PMCG.Messaging.Client.Interactive
{
	public class Bus
	{
		public void Run_Where_We_Instantiate_And_Try_To_Connect_To_Non_Existent_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:25672/");	// Wrong port number
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Allow time for connection attempt to fail, check bus state which should be disconnected");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Instantiate_And_Instruct_To_Stop_The_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Instruct_To_Stop_The_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Instruct_To_Close_The_Connection_Using_The_DashBoard()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Close()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>("test.exchange.1", typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					"test.queue.1",
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), null, "...", 1);
			_SUT.Publish(_message);

			Console.WriteLine("Hit enter to display captured message Id");
			Console.ReadLine();
			Console.WriteLine("Captured message Id [{0}]", _capturedMessageId);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>("test.exchange.1", typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; },
					"test.exchange.1");
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "...", 1);
			_SUT.Publish(_message);

			Console.WriteLine("Hit enter to display captured message Id");
			Console.ReadLine();
			Console.WriteLine("Captured message Id [{0}]", _capturedMessageId);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_Multiple_Messages_And_Consume_For_The_Same_Messsages()
		{
			var _numberOfMessagesToPublish = 2500;
			var _receivedMessages = new ConcurrentStack<MyEvent>();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add("amqp://guest:guest@localhost:5672/");
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>("test.exchange.1", typeof(MyEvent).Name + "v1")
				.RegisterConsumer<MyEvent>(
					"test.queue.1",
					typeof(MyEvent).Name,
					message => { _receivedMessages.Push(message); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish messages {0}", DateTime.Now);
			Console.ReadLine();
			Console.WriteLine("About to publish messages {0}", DateTime.Now);
			for (var _sequence = 1; _sequence <= _numberOfMessagesToPublish; _sequence++)
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "...", _sequence);
				_SUT.Publish(_message);
				Thread.Sleep(100);
			}
			Console.WriteLine("Completed publishing messages {0}", DateTime.Now);

			Console.WriteLine("Hit enter to verify recieved messages {0}", DateTime.Now);
			var _stopwatch = Stopwatch.StartNew();
			while (_receivedMessages.Count < _numberOfMessagesToPublish)
			{
				Console.WriteLine("{0} Received message count {1}", DateTime.Now, _receivedMessages.Count);
				Thread.Sleep(100);
			}
			_stopwatch.Stop();
			Console.WriteLine("{0} Received message count {1}", _stopwatch.ElapsedMilliseconds, _receivedMessages.Count);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}
	}
}
