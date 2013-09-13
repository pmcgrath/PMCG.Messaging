using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	class Program
	{
		static void Main()
		{
			new Subscribers().Run_Where_We_Instruct_To_Stop_The_Broker();
			Console.ReadLine();
		}
	}
}
