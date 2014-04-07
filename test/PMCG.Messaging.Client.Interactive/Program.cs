using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.Linq;


namespace PMCG.Messaging.Client.Interactive
{
	class Program
	{
		static void Main()
		{
			XmlConfigurator.Configure();
			GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;	// See http://stackoverflow.com/questions/2075603/log4net-process-id-information

			//new Bus().Run_Where_A_Consumer_Also_Publishes_A_Message_Consumer_Handler_Uses_A_Closure_To_Allow_Publishing_Using_The_Same_Bus_Reference();
			//new Bus().Run_Where_We_Attempt_A_Publication_Timeout();
			//new Bus().Run_Where_We_Connect_And_Instruct_To_Close_The_Connection_Using_The_DashBoard();
			//new Bus().Run_Where_We_Connect_And_Then_Close();
			//new Bus().Run_Where_We_Connect_And_Then_Instruct_To_Stop_The_Broker();
			//new Bus().Run_Where_We_Continuously_Publish_Handling_All_Results();
			//new Bus().Run_Where_We_Continuously_Publish_Until_Program_Killed();
			//new Bus().Run_Where_We_Instantiate_And_Instruct_To_Stop_The_Broker();
			//new Bus().Run_Where_We_Instantiate_And_Try_To_Connect_To_Non_Existent_Broker();
			//new Bus().Run_Where_We_Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange();
			//new Bus().Run_Where_We_Publish_1000_Messages_Waiting_On_Result();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue();
			//new Bus().Run_Where_We_Publish_A_Message_Subject_To_A_Timeout_And_Consume_The_Same_Messsage();
			//new Bus().Run_Where_We_Publish_A_Message_To_Two_Exchanges_No_Consumption_For_The_Same_Messsage();
			//new Bus().Run_Where_We_Publish_A_Null_Message_Results_In_An_Exception();
			new Bus().Run_Where_We_Publish_Multiple_Messages_And_Consume_For_The_Same_Messsages();
			//new Bus().Run_Where_We_Transition_Between_States_By_Instructing_The_Broker();

			//new ConnectionManager().Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server();

			//new Publisher().Run_Where_We_Batch_Publish_Messages_Waiting_For_Batch_Completion_Each_Time();
			//new Publisher().Run_Where_We_Instruct_To_Stop_The_Broker();
			//new Publisher().Run_Where_We_Publish_A_Message_To_A_Non_Existent_Exchange_Will_Close_The_Internal_Channel();
			//new Publisher().Run_Where_We_Publish_Messages_Waiting_For_Completion_Each_Time();
			
			//new Consumer().Run_Where_We_Create_A_Transient_Queue_And_Then_Close_Connection();
			//new Consumers().Run_Where_We_Instruct_To_Stop_The_Broker();
		}
	}
}
