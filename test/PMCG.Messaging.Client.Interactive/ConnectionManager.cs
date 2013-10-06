using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.Utility;
using System;


namespace PMCG.Messaging.Client.Interactive
{
	public class ConnectionManager
	{
		public void Run_Open()
		{
			var _logger = new ConsoleLogger();

			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				_logger,
				"amqp://guest:guest@localhost:5672/",
				TimeSpan.FromSeconds(4));

			_SUT.Open();
		}


		public void Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server()
		{
			var _logger = new ConsoleLogger();

			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				_logger,
				"amqp://guest:guest@localhost:5672/",
				TimeSpan.FromSeconds(4));

			_SUT.Open();

			Console.WriteLine("Opened");
		}
	}
}
