using System;


namespace PMCG.Messaging.Client.Utility
{
	public interface ILog
	{
		void DebugFormat(
			string formatMessage,
			params object[] arguments);

		void Info();

		void Info(
			string message);

		void InfoFormat(
			string formatMessage,
			params object[] arguments);

		void ErrorFormat(
			string formatMessage,
			params object[] arguments);

		void WarnFormat(
			string formatMessage,
			params object[] arguments);
	}
}