using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.RabbitMQ
{
	public interface IConnectionManager
	{
		IConnection Connection { get; }

		event EventHandler<ConnectionDisconnectedEventArgs> Disconnected;

		void Open();

		void Close();
	}
}