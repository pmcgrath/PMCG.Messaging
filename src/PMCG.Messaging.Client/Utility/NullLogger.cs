using System;
using System.Diagnostics;
using System.Threading;


namespace PMCG.Messaging.Client.Utility
{
	public class NullLogger : ILog
	{
		public void DebugFormat(
			string formatMessage,
			params object[] arguments)
		{
		}


		public void Info()
		{
		}


		public void Info(
			string message)
		{
		}


		public void InfoFormat(
			string formatMessage,
			params object[] arguments)
		{
		}


		public void ErrorFormat(
			string formatMessage,
			params object[] arguments)
		{
		}


		public void WarnFormat(
			string formatMessage,
			params object[] arguments)
		{
		}
	}
}