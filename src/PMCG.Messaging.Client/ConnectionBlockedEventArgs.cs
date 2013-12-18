using System;


namespace PMCG.Messaging.Client
{
	public class ConnectionBlockedEventArgs
	{
		public readonly string Reason;


		public ConnectionBlockedEventArgs(
			string reason)
		{
			this.Reason = reason;
		}
	}
}