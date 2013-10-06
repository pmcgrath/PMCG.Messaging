using System;
using System.Diagnostics;
using System.Threading;


namespace PMCG.Messaging.Client.Utility
{
	public class ConsoleLogger : ILog
	{
		public void DebugFormat(
			string formatMessage,
			params object[] arguments)
		{
			this.Write("DEBUG", string.Format(formatMessage, arguments));
		}


		public void Info()
		{
			this.Write("INFO", string.Empty);
		}


		public void Info(
			string message)
		{
			this.Write("INFO", message);
		}


		public void InfoFormat(
			string formatMessage,
			params object[] arguments)
		{
			this.Write("INFO", string.Format(formatMessage, arguments));
		}


		public void ErrorFormat(
			string formatMessage,
			params object[] arguments)
		{
			this.Write("ERROR", string.Format(formatMessage, arguments));
		}


		public void WarnFormat(
			string formatMessage,
			params object[] arguments)
		{
			this.Write("WARN", string.Format(formatMessage, arguments));
		}


		private void Write(
			string level,
			string message)
		{
			var _callingMethod = new StackTrace().GetFrame(2).GetMethod();
			Console.WriteLine("{0:hh:mm:ss} {1} {2} {3}.{4} {5}",
				DateTime.UtcNow,
				level,
				Thread.CurrentThread.ManagedThreadId,
				_callingMethod.DeclaringType.Name,
				_callingMethod.Name, message);
		}
	}
}