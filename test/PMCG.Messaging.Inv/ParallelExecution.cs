using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class ParallelExecution
	{
		public void Run()
		{
			var _tasks = new List<System.Threading.Tasks.Task<bool>>();

			try
			{
				Parallel.ForEach(new[] { 11, 2, 33 }, n => _tasks.Add(DoIt2(n)));
			}
			catch (Exception theException)
			{
				Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");
				Console.WriteLine("Non task exception : {0}", theException);
			}

			Task.WhenAll(_tasks).ContinueWith(taskResults =>
				{
					Console.WriteLine("Finished");
					if (taskResults.IsFaulted)
					{
						Console.WriteLine("Faulted : {0}", taskResults.Exception);
					}
				});
			Console.ReadLine();
		}


		public static Task<bool> DoIt2(
			int number)
		{
			Console.WriteLine("{0} DoIt2 Starting", System.Threading.Thread.CurrentThread.ManagedThreadId);
			var _result = new TaskCompletionSource<bool>();

			if (number == 2) { throw new ApplicationException("Pre-condition exception !"); }

			Thread.Sleep(1000);
			
			if (number == 3)	{ _result.SetException(new ApplicationException("Task exception !")); }
			else				{ _result.SetResult(true); }
			
			Console.WriteLine("{0} DoIt2 Exiting", System.Threading.Thread.CurrentThread.ManagedThreadId);
			return _result.Task;
		}
	}
}
