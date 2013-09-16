using PMCG.Messaging.RabbitMQ.Configuration;
using PMCG.Messaging.RabbitMQ.Utility;
using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	public class ConnectionManager
	{
		public void Run_Open()
		{
			var _logger = new ConsoleLogger();

			var _SUT = new PMCG.Messaging.RabbitMQ.ConnectionManager(
				_logger,
				"amqp://guest:guest@localhost:5672/dev",
				TimeSpan.FromSeconds(4));

			_SUT.Open();
		}


		public void Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server()
		{
			var _logger = new ConsoleLogger();

			var _SUT = new PMCG.Messaging.RabbitMQ.ConnectionManager(
				_logger,
				"amqp://guest:guest@localhost:5672/dev",
				TimeSpan.FromSeconds(4));

			_SUT.Open();

			Console.WriteLine("Opened");
		}
	}
}
