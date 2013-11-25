using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class TaskInv2
	{
		public void RunCase()
		{
			try
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to invoke", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				var _invokeResult = this.RunMultipleTasksWithSingleResult();

				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to wait", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				if (!_invokeResult.Wait(TimeSpan.FromSeconds(10)))
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] Timed out !", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				}
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Done waiting", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				if (_invokeResult.IsCompleted)
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}]        Result is {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, _invokeResult.Result);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] Error : {2} {3}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, exception.GetType(), exception.Message);
			}

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		public Task<bool> RunMultipleTasksWithSingleResult()
		{
			var _result = new TaskCompletionSource<bool>();

			var _arguments = new []
				{
					new DoWorkArguments { Sequence = 1, SleepInterval = TimeSpan.FromMilliseconds(5000), ResultToReturn = false, ShouldThrowException = false },
					new DoWorkArguments { Sequence = 2, SleepInterval = TimeSpan.FromMilliseconds(25), ResultToReturn = true, ShouldThrowException = true },
					new DoWorkArguments { Sequence = 3, SleepInterval = TimeSpan.FromMilliseconds(50), ResultToReturn = true, ShouldThrowException = false }
				};
			
			var _tasks = new List<Task<bool>>();
			Parallel.ForEach(_arguments, argument => _tasks.Add(this.DoWork(argument)));

			Task.WhenAll(_tasks).ContinueWith(taskResults =>
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] ---> Processing result ({2})", DateTime.Now, Thread.CurrentThread.ManagedThreadId, taskResults.Status);
					if (taskResults.IsFaulted)
					{
						_result.SetException(taskResults.Exception);
					}
					else
					{
						var _allGood = taskResults.Result.All(result => result);
						_result.SetResult(_allGood);
					}
				});

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to return task completion result", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			return _result.Task;
		}



		private Task<bool> DoWork(
			DoWorkArguments arguments)
		{
			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Doing work for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, arguments.Sequence);
			var _result = new TaskCompletionSource<bool>();

			new Task(() =>
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] :: Starting for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, arguments.Sequence);
					Thread.Sleep(arguments.SleepInterval);

					if (arguments.ShouldThrowException)
					{
						Console.WriteLine("{0:mm:ss.ffffff} [{1}] :: Sequence {2} is exceptional !", DateTime.Now, Thread.CurrentThread.ManagedThreadId, arguments.Sequence);
						var _exception = new ApplicationException("Bang !");
						_result.SetException(_exception);
					}
					else
					{
						_result.SetResult(arguments.ResultToReturn);
					}
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] :: Completed for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, arguments.Sequence);
				}).Start();

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Exiting for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, arguments.Sequence);
			return _result.Task;
		}
	}


	public class DoWorkArguments
	{
		public int Sequence;
		public TimeSpan SleepInterval;
		public bool ResultToReturn;
		public bool ShouldThrowException;
	}
}
