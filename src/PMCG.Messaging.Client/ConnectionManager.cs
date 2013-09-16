using PMCG.Messaging.RabbitMQ.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Threading;


namespace PMCG.Messaging.RabbitMQ
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly ILog c_logger;
		private readonly ConnectionFactory c_connectionFactory;
		private readonly TimeSpan c_reconnectionPauseInterval;


		private IConnection c_connection;
		private bool c_isCloseRequested;


		public event EventHandler<ConnectionDisconnectedEventArgs> Disconnected = (sender, eventArgs) => { };


		public bool IsOpen { get { return this.c_connection != null && this.c_connection.IsOpen; } }
		public IConnection Connection { get { return this.c_connection; } }


		public ConnectionManager(
			ILog logger,
			string connectionUri,
			TimeSpan reconnectionPauseInterval)
		{
			Check.RequireArgumentNotNull("logger", logger);
			Check.RequireArgumentNotEmpty("connectionUri", connectionUri);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.Ticks > 0);

			this.c_logger = logger;
			this.c_reconnectionPauseInterval = reconnectionPauseInterval;

			this.c_connectionFactory = new ConnectionFactory { Uri = connectionUri };

			this.c_logger.Info("Completed");
		}


		public void Open()
		{
			this.c_logger.Info();

			this.c_isCloseRequested = false;
			while (!this.c_isCloseRequested)
			{
				try
				{
					this.c_logger.Info("Trying to connect");
					this.c_connection = this.c_connectionFactory.CreateConnection();
					this.c_connection.ConnectionShutdown += this.OnConnectionShutdown;
					break;
				}
				catch (SocketException exception)
				{
					this.c_logger.InfoFormat("Failed to connect {0} {1}", exception.GetType().Name, exception.Message);
				}
				catch (BrokerUnreachableException exception)
				{
					this.c_logger.InfoFormat("Failed to connect {0} {1}", exception.GetType().Name, exception.Message);
				}

				if (this.c_isCloseRequested) { return; }
				Thread.Sleep(this.c_reconnectionPauseInterval);
			}

			this.c_logger.Info("Completed");
		}


		public void Close()
		{
			this.c_logger.Info();

			this.c_isCloseRequested = true;
			if (this.IsOpen)
			{
				this.c_connection.ConnectionShutdown -= this.OnConnectionShutdown;
				this.c_connection.Close();
			}

			this.c_logger.Info("Completed");
		}


		private void OnConnectionShutdown(
			IConnection connection,
			ShutdownEventArgs reason)
		{
			this.c_logger.Info();
			this.Disconnected(null, new ConnectionDisconnectedEventArgs(reason.ReplyCode, reason.ReplyText));
		}
	}
}