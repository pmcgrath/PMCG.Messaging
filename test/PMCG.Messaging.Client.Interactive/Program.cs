using System;


namespace PMCG.Messaging.Client.Interactive
{
	class Program
	{
		static void Main()
		{
			//new Publisher().Run_Where_We_Instruct_To_Stop_The_Broker();
			//new Publisher().Run_Where_We_Publish_Messages();
			//new Publisher().Run_Where_We_Publish_Async_Messages();
			//new Publisher().Run_Where_We_Publish_Async_A_Message_To_A_Non_Existent_Exchange_Will_Close_The_Internal_Channel();

			//new ConnectionManager().Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server();

			//new Consumer().Run_Where_We_Create_A_Transient_Queue_And_Then_Close_Connection();

			//new Consumers().Run_Where_We_Instruct_To_Stop_The_Broker();

			//new Bus().Run_Where_We_Instantiate_And_Try_To_Connect_To_Non_Existent_Broker();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage();
			//new Bus().Run_Where_We_Publish_Multiple_Messages_And_Consume_For_The_Same_Messsages();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue();
			new Bus().Run_Where_We_Continuously_Publish_Until_Program_Killed();
			return;



			try
			{
				//System.Threading.Tasks.Parallel.ForEach(System.Linq.Enumerable.Range(1, 15), n => DoWork(n));

				var _results = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
				System.Threading.Tasks.Parallel.ForEach(System.Linq.Enumerable.Range(1, 150), n => _results.Add(DoWorkAsync(n)));
				System.Threading.Tasks.Task.WaitAll(_results.ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine("{0} {1} Exc {2}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, e);
			}
			Console.WriteLine("{0} {1} Done", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}

		
		public static void DoWork(
			int number)
		{
			Console.WriteLine("{0} {1} Number {2} starting", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number);
			System.Threading.Thread.Sleep(100);
			if (number == 5) throw new ApplicationException("!");
			Console.WriteLine("{0} {1} Number {2} completed", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number);
		}


		public static System.Threading.Tasks.Task DoWorkAsync(
			int number)
		{
			var _result = new System.Threading.Tasks.TaskCompletionSource<bool>();

			Console.WriteLine("{0} {1} Number {2} starting", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number);

			new System.Threading.Timer((state) => 
				{
					try
					{
						Console.WriteLine("{0} {1} Number {2} ABOUT TO SET RESULT", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number);
						if (number == 30) throw new ApplicationException(string.Format("EXC    !! {0} {1} Number {2} starting", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number));
						if (number == 60) throw new ApplicationException(string.Format("EXC    !! {0} {1} Number {2} starting", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number));
					}
					catch (Exception e)
					{
						throw;
						//_result.SetException(e);
						//return;
					}
					_result.SetResult(true);
				},
				null,
				100,
				System.Threading.Timeout.Infinite);

			Console.WriteLine("{0} {1} Number {2} completed", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, number);
			return _result.Task;
		}
	}
}
