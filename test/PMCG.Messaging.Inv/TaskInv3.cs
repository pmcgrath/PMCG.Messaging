using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class TaskInv3
	{
		public void Run_Where_We_Wait_On_IsCompleted_No_Unhandled_Exception()
		{
			var _task = this.DoWork();
			try
			{
				while (!_task.IsCompleted)
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] Waiting", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
					Thread.Sleep(200);
				}
			}
			catch (Exception theException)
			{
				Console.WriteLine("We never get here");
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		public void Run_Where_We_Call_Wait_Results_In_Exception_Handler_Being_Invoked()
		{

			var _task = this.DoWork();

			try
			{
				_task.Wait();
			}
			catch (Exception theException)
			{
				Console.WriteLine("We get here");
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}

	
		private Task<bool> DoWork()
		{
			var _result = new TaskCompletionSource<bool>();

			var _timer = new Timer(state => _result.SetException(new ApplicationException("AAAAA !!!")), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0));

			return _result.Task;
		}
	}
}