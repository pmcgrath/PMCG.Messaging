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
						Console.WriteLine("{0:mm:ss.ffffff} [{1}]        Result is {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, _result);
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


		public void RunCase2()
		{
			try
			{
				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to invoke", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				var _invokeResult = this.RunMultipleTasksWithSingleResult();

				Console.WriteLine("{0:mm:ss.ffffff} [{1}] About to wait", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
				if (!_invokeResult.Wait(TimeSpan.FromSeconds(20)))
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


		public Task<bool[]> RunMultipleTasks()
		{
			var _tasks = new []
				{
					this.DoWork(1, TimeSpan.FromMilliseconds(5000), resultToReturn: true, shouldThrowException: false),
					this.DoWork(2, TimeSpan.FromMilliseconds(25), resultToReturn: true, shouldThrowException: false),
					this.DoWork(3, TimeSpan.FromMilliseconds(50), resultToReturn: true, shouldThrowException: false),
				};

			return Task.WhenAll(_tasks);
		}


		public Task<bool> RunMultipleTasksWithSingleResult()
		{
			var _result = new TaskCompletionSource<bool>();

			var _tasks = new[]
				{
					this.DoWork(1, TimeSpan.FromMilliseconds(5000), resultToReturn: true, shouldThrowException: false),
					this.DoWork(2, TimeSpan.FromMilliseconds(25), resultToReturn: false, shouldThrowException: false),
					this.DoWork(3, TimeSpan.FromMilliseconds(50), resultToReturn: true, shouldThrowException: false),
				};

			Task.WhenAll(_tasks).ContinueWith(taskResults =>
				{
					Console.WriteLine("{0:mm:ss.ffffff} [{1}] ---> Processing result", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
					var _allGood = taskResults.Result.All(result => result);
					_result.SetResult(_allGood);
				});

			return _result.Task;
		}


		private Task<bool> DoWork(
			int sequence,
			TimeSpan sleepInterval,
			bool resultToReturn,
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

			_result.SetResult(resultToReturn);
			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Completed for sequence {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, sequence);

			return _result.Task;
		}
	}
}
