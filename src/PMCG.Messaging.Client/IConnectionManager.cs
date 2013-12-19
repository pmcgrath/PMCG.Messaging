using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.Client
{
	public interface IConnectionManager
	{
		IConnection Connection { get; }
		bool IsOpen { get; }


		event EventHandler<ConnectionBlockedEventArgs> Blocked;
		event EventHandler<ConnectionDisconnectedEventArgs> Disconnected;
		event EventHandler<EventArgs> Unblocked;


		void Open(
			uint numberOfTimesToTry = 0);

		void Close();
	}
}