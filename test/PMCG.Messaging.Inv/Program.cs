using System;


namespace PMCG.Messaging.Inv
{
	class Program
	{
		static void Main(
			string[] args)
		{
			new PuplisherConfirmsUsage().Run(50000);
			//new SyncPuplisherConfirms().Run(500);
			//new TopicsUsage().Run();
		}
	}
}
