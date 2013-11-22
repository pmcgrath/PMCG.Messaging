using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PMCG.Messaging.Inv
{
	public class PublisherConfirmsWithTasksInv
	{
		private readonly string c_connectionUri;
		private readonly string c_exchangeName;
		private readonly string c_queueName;
		private readonly int c_numberOfMessages;
		private readonly ConcurrentDictionary<ulong, TaskCompletionSource<string>> c_unconfirmedMessages;


		public PublisherConfirmsWithTasksInv(
			string connectionUri,
			string exchangeName,
			string queueName,
			int numberOfMessages)
		{
			this.c_connectionUri = connectionUri;
			this.c_exchangeName = exchangeName;
			this.c_queueName = queueName;
			this.c_numberOfMessages = numberOfMessages;

			this.c_unconfirmedMessages = new ConcurrentDictionary<ulong, TaskCompletionSource<string>>();
		}


		public static void Run(
			string[] args)
		{
			var _connectionUri = Configuration.LocalConnectionUri;
			var _exchangeName = Configuration.ExchangeName;
			var _queueName = Configuration.QueueName;
			var _numberOfMessages = 1500;

			Console.WriteLine("\r\n\r\nPersist messages async with tasks, hit any key to start"); Console.ReadKey();
			new PublisherConfirmsWithTasksInv(
				_connectionUri,
				_exchangeName,
				_queueName,
				_numberOfMessages).Run();

			Console.ReadLine();
		}


		private void Run()
		{
			var _connectionFactory = new ConnectionFactory { Uri = this.c_connectionUri };
			var _connection = _connectionFactory.CreateConnection();
			var _channel = _connection.CreateModel();
			_channel.ConfirmSelect();
			_channel.BasicAcks += this.OnChannelAck;
			_channel.BasicNacks += this.OnChannelNack;

			_channel.ExchangeDeclare(this.c_exchangeName, ExchangeType.Fanout, true, false, null);
			_channel.QueueDeclare(this.c_queueName, true, false, false, null);
			_channel.QueueBind(this.c_queueName, this.c_exchangeName, string.Empty, null);

			var _stopwatch = Stopwatch.StartNew();

			var _tasks = new Task[this.c_numberOfMessages];
			for (int _sequence = 1; _sequence <= this.c_numberOfMessages; _sequence++)
			{
				var _properties = _channel.CreateBasicProperties();
				_properties.ContentType = "text/plain";
				_properties.MessageId = Guid.NewGuid().ToString();
				_properties.DeliveryMode = 2;

				var _messageBodyContent = string.Format("Message published @ {0} with Id {1}", DateTime.Now, _properties.MessageId);
				var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

				var _taskCompletionSource = new TaskCompletionSource<string>(_messageBodyContent);
				this.c_unconfirmedMessages.TryAdd(_channel.NextPublishSeqNo, _taskCompletionSource);
				_channel.BasicPublish(this.c_exchangeName, string.Empty, _properties, _messageBody);
				_tasks[_sequence - 1] = _taskCompletionSource.Task;
			}

			Task.WaitAll(_tasks);
			_stopwatch.Stop();

			_connection.Close();
			Console.WriteLine("Unconfirmed messages count = {0}, elapsed time is {1}", this.c_unconfirmedMessages.Count, _stopwatch.ElapsedMilliseconds);
		}


		private void OnChannelAck(
			IModel channel,
			BasicAckEventArgs args)
		{
			TaskCompletionSource<string> _taskCompletionSource;
			var _confirmedDeliveryTags = this.c_unconfirmedMessages.Keys.Where(deliveryTag => deliveryTag <= args.DeliveryTag);
			foreach (var _confirmedDeliveryTag in _confirmedDeliveryTags)
			{
				if (!this.c_unconfirmedMessages.TryRemove(_confirmedDeliveryTag, out _taskCompletionSource))
				{
					throw new ApplicationException("Could not remove delivery tag entry");
				}

				_taskCompletionSource.SetResult((string)_taskCompletionSource.Task.AsyncState);
			}
		}


		private void OnChannelNack(
			IModel channel,
			BasicNackEventArgs args)
		{
			TaskCompletionSource<string> _taskCompletionSource;
			var _confirmedDeliveryTags = this.c_unconfirmedMessages.Keys.Where(deliveryTag => deliveryTag <= args.DeliveryTag);
			foreach (var _confirmedDeliveryTag in _confirmedDeliveryTags)
			{
				if (!this.c_unconfirmedMessages.TryRemove(_confirmedDeliveryTag, out _taskCompletionSource))
				{
					throw new ApplicationException("Could not remove delivery tag entry");
				}

				_taskCompletionSource.SetResult((string)_taskCompletionSource.Task.AsyncState);
			}
		}
	}
}
