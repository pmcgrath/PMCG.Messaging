using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


namespace PMCG.Messaging.Inv
{
	public class PuplisherConfirmsUsage
	{
		private readonly string c_connectionUri = "amqp://guest:guest@localhost:5672/";
		private readonly string c_exchangeName = "test_publisher_confirms";
		private readonly string c_queueName = "test_publisher_confirms";
		private readonly Action<string> c_writeLog = message => Console.WriteLine("{0:hh:mm:ss.ffff} {1,3} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, message);


		private IConnection c_connection;
		private ConcurrentDictionary<ulong, string> c_unconfirmedMessages;


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

			this.c_writeLog("Waiting on basic acks completion");
			this.WaitOnBasicAcksCompletion();
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
			this.c_unconfirmedMessages = new ConcurrentDictionary<ulong, string>();

			var _channel = this.c_connection.CreateModel();
			_channel.ConfirmSelect();
			_channel.BasicAcks += this.OnChannelAck;

			for(int _sequence = 1; _sequence <= numberOfMessages; _sequence++)
			{
				var _properties = _channel.CreateBasicProperties();
				_properties.ContentType = "text/plain";
				_properties.DeliveryMode = 2;
				_properties.MessageId = Guid.NewGuid().ToString();

				var _messageBodyContent = string.Format("Message published @ {0} with Id {1}", DateTime.Now, _properties.MessageId);
				var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

				this.c_writeLog(string.Format("About to publish message ({0})", _messageBodyContent));
				this.c_unconfirmedMessages.TryAdd(_channel.NextPublishSeqNo, _messageBodyContent);
				_channel.BasicPublish(this.c_exchangeName, string.Empty, _properties, _messageBody);
			};

			this.c_writeLog("Completed publishing");
		}


		private void OnChannelAck(
			IModel channel,
			BasicAckEventArgs args)
		{
			this.c_writeLog(string.Format("On channel ack : Is multiple {0}, delivery tag {1}", args.Multiple, args.DeliveryTag));
			var _confirmedDeliveryTags = this.c_unconfirmedMessages.Keys.Where(deliveryTag => deliveryTag <= args.DeliveryTag);
			var _removedMessage = string.Empty;
			foreach (var _confirmedDeliveryTag in _confirmedDeliveryTags)
			{
				if (!this.c_unconfirmedMessages.TryRemove(_confirmedDeliveryTag, out _removedMessage))
				{
					throw new ApplicationException("Could not remove delivery tag entry");
				}
			}
		}


		private void WaitOnBasicAcksCompletion()
		{
			while (this.c_unconfirmedMessages.Count > 0)
			{
				this.c_writeLog("Waiting on basic acks completion");
			}
		}


		private void CloseConnection()
		{
			this.c_connection.Close();
		}
	}
}
