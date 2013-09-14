using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	public class Bus
	{
		public void Run_Where_We_Instantiate_And_Instruct_To_Stop_The_Broker()
		{
			var _logger = new ConsoleLogger();
			
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "amqp://guest:guest@localhost:5672/dev";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.RabbitMQ.Bus(_logger, _busConfigurationBuilder.Build());

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Instruct_To_Stop_The_Broker()
		{
			var _logger = new ConsoleLogger();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "amqp://guest:guest@localhost:5672/dev";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.RabbitMQ.Bus(_logger, _busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Instruct_To_Close_The_Connection_Using_The_DashBoard()
		{
			var _logger = new ConsoleLogger();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "amqp://guest:guest@localhost:5672/dev";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.RabbitMQ.Bus(_logger, _busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Close()
		{
			var _logger = new ConsoleLogger();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "amqp://guest:guest@localhost:5672/dev";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";

			var _SUT = new PMCG.Messaging.RabbitMQ.Bus(_logger, _busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_And_Subscribe_For_The_Same_Messsage()
		{
			var _capturedMessageId = string.Empty;

			var _logger = new ConsoleLogger();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "amqp://guest:guest@localhost:5672/dev";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";
			_busConfigurationBuilder
				.RegisterPublication<TheEvent>("pcs.offerevents", typeof(TheEvent).Name)
				.RegisterSubscription<TheEvent>("pcs.offerevents.fxs", typeof(TheEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return MessageSubscriptionActionResult.Completed; });
			var _SUT = new PMCG.Messaging.RabbitMQ.Bus(_logger, _busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new TheEvent(Guid.NewGuid(), "...");
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
	}
}
