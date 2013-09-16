using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.Client
{
	public interface IConnectionManager
	{
		IConnection Connection { get; }

		event EventHandler<ConnectionDisconnectedEventArgs> Disconnected;

		void Open();

		void Close();
	}
}