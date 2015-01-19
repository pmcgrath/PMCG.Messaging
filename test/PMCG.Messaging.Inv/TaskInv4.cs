using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class TaskInv4
	{
		private CancellationToken c_cancellationToken;


		public void Run()
		{
			var _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			this.c_cancellationToken = _cancellationTokenSource.Token;
			this.DoWork();

			Console.WriteLine("{0:mm:ss.ffffff} [{1}] Hit enter to exit", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}


		private Task DoWork()
		{
			var _task = new Task(() =>
				{
					var _index = 0;
					while (true)
					{
						Console.WriteLine("{0:mm:ss.ffffff} [{1}] In loop", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
						Thread.Sleep(1000);

						if (_index == 2) throw new ApplicationException("From within task");

						this.c_cancellationToken.ThrowIfCancellationRequested();
						_index++;
					}
				});
			_task.Start();

			return _task;
		}
	}
}
