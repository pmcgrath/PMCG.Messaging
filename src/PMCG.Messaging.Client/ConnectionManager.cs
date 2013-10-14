using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;


namespace PMCG.Messaging.Client
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly ILog c_logger;
		private readonly IEnumerable<string> c_connectionUris;
		private readonly TimeSpan c_reconnectionPauseInterval;


		private IConnection c_connection;
		private bool c_isCloseRequested;


		public event EventHandler<ConnectionDisconnectedEventArgs> Disconnected = (sender, eventArgs) => { };


		public bool IsOpen { get { return this.c_connection != null && this.c_connection.IsOpen; } }
		public IConnection Connection { get { return this.c_connection; } }


		public ConnectionManager(
			ILog logger,
			IEnumerable<string> connectionUris,
			TimeSpan reconnectionPauseInterval)
		{
			Check.RequireArgumentNotNull("logger", logger);
			Check.RequireArgumentNotEmptyAndNonEmptyItems("connectionUris", connectionUris);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.Ticks > 0);

			this.c_logger = logger;
			this.c_connectionUris = connectionUris;
			this.c_reconnectionPauseInterval = reconnectionPauseInterval;

			this.c_logger.Info("ctor Completed");
		}


		public void Open(
			uint numberOfTimesToTry = 0)
		{
			this.c_logger.Info("Open Starting");
			Check.Ensure(!this.IsOpen, "Connection is already open");

			var _attemptSequence = 1;
			this.c_isCloseRequested = false;
			while (!this.c_isCloseRequested)
			{
				try
				{
					this.c_logger.InfoFormat("Open Trying to connect, sequence {0}", _attemptSequence);
					foreach (var _connectionUri in this.c_connectionUris)
					{
						var _connectionFactory = new ConnectionFactory { Uri = _connectionUri };
						var _connectionInfo = string.Format("Host {0}, port {1}, vhost {2}", _connectionFactory.HostName, _connectionFactory.Port, _connectionFactory.VirtualHost);
						this.c_logger.InfoFormat("Open Attempting to connect to ({0}), sequence {1}", _connectionInfo, _attemptSequence);
						this.c_connection = _connectionFactory.CreateConnection();
						this.c_connection.ConnectionShutdown += this.OnConnectionShutdown;
						this.c_logger.InfoFormat("Open Connected to ({0}), sequence {1}", _connectionInfo, _attemptSequence);
						break;
					}

					if (this.IsOpen) { break; }
				}
				catch (SocketException exception)
				{
					this.c_logger.WarnFormat("Open Failed to connect {0} - {1} {2}", _attemptSequence, exception.GetType().Name, exception.Message);
				}
				catch (BrokerUnreachableException exception)
				{
					this.c_logger.WarnFormat("Open Failed to connect, sequence {0} - {1} {2}", _attemptSequence, exception.GetType().Name, exception.Message);
				}

				if (this.c_isCloseRequested) { return; }
				if (numberOfTimesToTry > 0 && _attemptSequence == numberOfTimesToTry) { return; }

				Thread.Sleep(this.c_reconnectionPauseInterval);
				_attemptSequence++;
			}

			this.c_logger.Info("Open Completed");
		}


		public void Close()
		{
			this.c_logger.Info("Close Starting");

			this.c_isCloseRequested = true;
			if (this.IsOpen)
			{
				this.c_connection.ConnectionShutdown -= this.OnConnectionShutdown;
				this.c_connection.Close();
			}

			this.c_logger.Info("Close Completed");
		}


		private void OnConnectionShutdown(
			IConnection connection,
			ShutdownEventArgs reason)
		{
			this.c_logger.Info("OnConnectionShutdown Starting");
			this.Disconnected(null, new ConnectionDisconnectedEventArgs(reason.ReplyCode, reason.ReplyText));
			this.c_logger.Info("OnConnectionShutdown Completed");
		}
	}
}