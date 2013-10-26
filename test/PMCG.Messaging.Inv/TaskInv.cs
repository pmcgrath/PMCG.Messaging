using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class TaskInv
	{
		public void RunCase1()
		{
			var _tasks = new []
				{
					new Task<Task<bool>>(() => this.DoWork(1, TimeSpan.FromMilliseconds(50), false)),
					new Task<Task<bool>>(() => this.DoWork(2, TimeSpan.FromMilliseconds(25), false)),
					new Task<Task<bool>>(() => this.DoWork(3, TimeSpan.FromMilliseconds(50), true)),
					new Task<Task<bool>>(() => this.DoWork(4, TimeSpan.FromMilliseconds(20), false)),
					new Task<Task<bool>>(() => this.DoWork(5, TimeSpan.FromMilliseconds(20), false)),
					new Task<Task<bool>>(() => this.DoWork(6, TimeSpan.FromMilliseconds(20), false)),
					new Task<Task<bool>>(() => this.DoWork(7, TimeSpan.FromMilliseconds(20), false)),
					new Task<Task<bool>>(() => this.DoWork(8, TimeSpan.FromMilliseconds(20), false)),
					new Task<Task<bool>>(() => this.DoWork(9, TimeSpan.FromMilliseconds(20), true))
				};

			try
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to start", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				Parallel.ForEach(_tasks, task => task.Start());
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Waiting on tasks", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				Task.WaitAll(_tasks);
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Tasks completed !", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			}
			catch (Exception exception)
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Error : {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, exception);
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		private Task<bool> DoWork(
			int sequence,
			TimeSpan sleepInterval,
			bool shouldThrowException)
		{
			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Starting for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, sequence);
			var _result = new TaskCompletionSource<bool>();

			Thread.Sleep(sleepInterval);

			if (shouldThrowException)
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Sequence {2} is exceptional !", DateTime.Now, Thread.CurrentThread.ManagedThreadId, sequence);
				throw new ApplicationException("Bang !");
			}

			_result.SetResult(true);
			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Completed for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, sequence);

			return _result.Task;
		}
	}
}
