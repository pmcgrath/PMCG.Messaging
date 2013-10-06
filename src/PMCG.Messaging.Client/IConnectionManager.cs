using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.Client
{
	public interface IConnectionManager
	{
		IConnection Connection { get; }
		bool IsOpen { get; }

		event EventHandler<ConnectionDisconnectedEventArgs> Disconnected;

		void Open(
			uint numberOfTimesToTry = 0);

		void Close();
	}
}