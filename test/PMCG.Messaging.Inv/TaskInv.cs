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
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Error : {2} {3}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, exception.GetType(), exception.Message);
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		public void RunCase2()
		{
			try
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to invoke", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				var _invokeResult = this.RunMultipleTasks();

				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to wait", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				if (!_invokeResult.Wait(TimeSpan.FromSeconds(20)))
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] Timed out !", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				}
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Done waiting", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				if (_invokeResult.IsCompleted)
				{
					foreach (var _result in _invokeResult.Result)
					{
						Console.WriteLine("{0:mm:ss.ffffff} [{1}]        Result is {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, _result.Result);
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Error : {2} {3}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, exception.GetType(), exception.Message);
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		public Task<Task<bool>[]> RunMultipleTasks()
		{
			var _tasks = new[]
				{
					new Task<Task<bool>>(() => this.DoWork(1, TimeSpan.FromMilliseconds(5000), true)),
					new Task<Task<bool>>(() => this.DoWork(2, TimeSpan.FromMilliseconds(25), false)),
					new Task<Task<bool>>(() => this.DoWork(3, TimeSpan.FromMilliseconds(50), false))
				};

			Parallel.ForEach(_tasks, task => task.Start());
			var _result = Task.WhenAll(_tasks);

			return _result;
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
