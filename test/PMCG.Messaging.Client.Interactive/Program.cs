using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	class Program
	{
		static void Main()
		{
			//new Subscribers().Run_Where_We_Instruct_To_Stop_The_Broker();
			//new Bus().Run_Where_We_Publish_A_Message_And_Subscribe_For_The_Same_Messsage();
			new Bus().Run_Where_We_Publish_Multiple_Messages_And_Subscribe_For_The_Same_Messsages();
			//new ConnectionManager().Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server();
		}
	}
}
