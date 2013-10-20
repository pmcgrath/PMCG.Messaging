using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Text;


namespace PMCG.Messaging.Inv
{
	public class SyncPublisherConfirms
	{
		private readonly string c_connectionUri = "amqp://guest:guest@localhost:5672/";
		private readonly string c_exchangeName = "test_publisher_confirms";
		private readonly string c_queueName = "test_publisher_confirms";
		private readonly TimeSpan c_waitTimeOut = TimeSpan.FromMilliseconds(5);
		private readonly Action<string> c_writeLog = message => Console.WriteLine("{0:hh:mm:ss.ffff} {1,3} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, message);


		private IConnection c_connection;


		public void Run(
			int numberOfMessages)
		{
			this.c_writeLog("Opening connection");
			this.OpenConnection();

			this.c_writeLog("Ensuring configuration exists");
			this.EnsureConfigurationExists();

			var _stopwatch = Stopwatch.StartNew();
			this.c_writeLog("Running publisher");
			this.RunPublisher(numberOfMessages);
			_stopwatch.Stop();
			this.c_writeLog(string.Format("Elapsed time is {0}", _stopwatch.ElapsedMilliseconds));

			Console.WriteLine("Hit enter to close connection and exit");
			Console.ReadLine();
			this.c_writeLog("Closing connection");
			this.CloseConnection();
		}


		private void OpenConnection()
		{
			var _connectionFactory = new ConnectionFactory { Uri = this.c_connectionUri };
			this.c_connection = _connectionFactory.CreateConnection();
		}


		private void EnsureConfigurationExists()
		{
			var _channel = this.c_connection.CreateModel();

			_channel.ExchangeDeclare(this.c_exchangeName, ExchangeType.Fanout, false, false, null);

			_channel.QueueDeclare(this.c_queueName, true, false, false, null);
			_channel.QueueBind(this.c_queueName, this.c_exchangeName, string.Empty, null);

			_channel.Close();
		}


		private void RunPublisher(
			int numberOfMessages)
		{
			var _channel = this.c_connection.CreateModel();
			_channel.ConfirmSelect();
			_channel.BasicAcks += (c, e) => Console.WriteLine("Acked {0} {1}", e.DeliveryTag, e.Multiple);

			for(int _sequence = 1; _sequence <= numberOfMessages; _sequence++)
			{
				var _properties = _channel.CreateBasicProperties();
				_properties.ContentType = "text/plain";
				_properties.DeliveryMode = 2;
				_properties.MessageId = Guid.NewGuid().ToString();

				var _messageBodyContent = string.Format("Message published @ {0} with Id {1}", DateTime.Now, _properties.MessageId);
				var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

				this.c_writeLog(string.Format("About to publish message ({0})", _messageBodyContent));
				_channel.BasicPublish(this.c_exchangeName, string.Empty, _properties, _messageBody);
				var _timedOut = false;
				var _confirmed = _channel.WaitForConfirms(this.c_waitTimeOut, out _timedOut);
				if (_timedOut)
				{
					this.c_writeLog(string.Format("Timed out on publish message ({0})", _messageBodyContent));
				}
			}

			this.c_writeLog("Completed publishing");
		}


		private void CloseConnection()
		{
			this.c_connection.Close();
		}
	}
}
