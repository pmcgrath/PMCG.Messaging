using System;


namespace PMCG.Messaging.Inv
{
	class Program
	{
		static void Main(
			string[] args)
		{
			//PublisherConfirmsInv.Run(null);
			//PublisherConfirmsWithTasksInv.Run(null);
			//new PublisherConfirmsUsage().Run(50000);
			//new SyncPublisherConfirms().Run(500);
			//new TopicsUsage().Run();
			//new TaskInv().RunCase1();
			//new TaskInv().RunCase2();
			//new TaskInv2().RunCase();
			new TaskInv4().Run();
			//new ParallelExecution().Run();
			//new TaskInv3().Run_Where_We_Wait_On_IsCompleted_No_Unhandled_Exception();
			//new TaskInv3().Run_Where_We_Call_Wait_Results_In_Exception_Handler_Being_Invoked();
		}
	}
}
