using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace PMCG.Messaging.Inv
{
	public class PublisherConfirmsInv
	{
		private readonly string c_connectionUri;
		private readonly string c_exchangeName;
		private readonly string c_queueName;
		private readonly int c_numberOfMessages;
		private readonly bool c_persistMessages;
		private readonly bool c_waitForConfirms;
		private readonly ConcurrentDictionary<ulong, string> c_unconfirmedMessages;


		public PublisherConfirmsInv(
			string connectionUri,
			string exchangeName,
			string queueName,
			int numberOfMessages,
			bool persistMessages,
			bool waitForConfirms)
		{
			this.c_connectionUri = connectionUri;
			this.c_exchangeName = exchangeName;
			this.c_queueName = queueName;
			this.c_numberOfMessages = numberOfMessages;
			this.c_persistMessages = persistMessages;
			this.c_waitForConfirms = waitForConfirms;

			this.c_unconfirmedMessages = new ConcurrentDictionary<ulong, string>();
		}


		public static void Run(
			string[] args)
		{
			var _connectionUri = Configuration.LocalConnectionUri;
			var _exchangeName = Configuration.ExchangeName;
			var _queueName = Configuration.QueueName;
			var _numberOfMessages = 1500;

			Console.WriteLine("\r\n\r\nDo not persist messages async, hit any key to start"); Console.ReadKey();
			new PublisherConfirmsInv(
				_connectionUri,
				_exchangeName,
				_queueName,
				_numberOfMessages,
				false,
				false).Run();

			Console.WriteLine("\r\n\r\nDo not persist messages sync, hit any key to start"); Console.ReadKey();
			new PublisherConfirmsInv(
				_connectionUri,
				_exchangeName,
				_queueName,
				_numberOfMessages,
				false,
				true).Run();

			Console.WriteLine("\r\n\r\nDo persist messages async, hit any key to start"); Console.ReadKey();
			new PublisherConfirmsInv(
				_connectionUri,
				_exchangeName,
				_queueName,
				_numberOfMessages,
				true,
				false).Run();

			Console.WriteLine("\r\n\r\nDo persist messages sync, hit any key to start"); Console.ReadKey();
			new PublisherConfirmsInv(
				_connectionUri,
				_exchangeName,
				_queueName,
				_numberOfMessages,
				true,
				true).Run();

			Console.ReadLine();
		}


		private void Run()
		{
			var _connectionFactory = new ConnectionFactory { Uri = this.c_connectionUri };
			var _connection = _connectionFactory.CreateConnection();
			var _channel = _connection.CreateModel();
			_channel.ConfirmSelect();
			if (this.c_waitForConfirms)
			{
				// Wait for confirm - we do not use the callback in this case
			}
			else
			{
				// Callback for case where waitForConfirms is false
				_channel.BasicAcks += this.OnChannelAck;
				_channel.BasicNacks += this.OnChannelNack;
			}

			_channel.ExchangeDeclare(this.c_exchangeName, ExchangeType.Fanout, true, false, null);
			_channel.QueueDeclare(this.c_queueName, true, false, false, null);
			_channel.QueueBind(this.c_queueName, this.c_exchangeName, string.Empty, null);

			var _stopwatch = Stopwatch.StartNew();

			for (int _sequence = 1; _sequence <= this.c_numberOfMessages; _sequence++)
			{
				var _properties = _channel.CreateBasicProperties();
				_properties.ContentType = "text/plain";
				_properties.MessageId = Guid.NewGuid().ToString();
				if (this.c_persistMessages)
				{
					// Persist
					_properties.DeliveryMode = 2;
				}
				else
				{
					// Do NOT Persist
					_properties.DeliveryMode = 1;
				}

				var _messageBodyContent = string.Format("Message published @ {0} with Id {1}", DateTime.Now, _properties.MessageId);
				var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

				if (this.c_waitForConfirms)
				{
					// Wait for confirm - lets wait until broker acks before continuing
					_channel.BasicPublish(this.c_exchangeName, string.Empty, _properties, _messageBody);
					_channel.WaitForConfirms();
				}
				else
				{
					// Do not wait for callback - add unconfirmed message to collection before publishing
					this.c_unconfirmedMessages.TryAdd(_channel.NextPublishSeqNo, _messageBodyContent);
					_channel.BasicPublish(this.c_exchangeName, string.Empty, _properties, _messageBody);
				}
			}

			if (this.c_waitForConfirms)
			{
				// Wait for confirm - have gotten a confirm on each publication already so nothing left to do at this stage
			}
			else
			{
				// Do not wait for callback - must wait until all messages have been confirmed
				while (this.c_unconfirmedMessages.Count > 0)
				{
					// Noop
				}
			}
			_stopwatch.Stop();

			_connection.Close();
			Console.WriteLine("Persist messages = {0}, wait for confirm = {1}, elapsed time is {2}", this.c_persistMessages, this.c_waitForConfirms, _stopwatch.ElapsedMilliseconds);
		}


		private void OnChannelAck(
			IModel channel,
			BasicAckEventArgs args)
		{
			var _removedMessage = string.Empty;
			var _confirmedDeliveryTags = this.c_unconfirmedMessages.Keys.Where(deliveryTag => deliveryTag <= args.DeliveryTag);
			foreach (var _confirmedDeliveryTag in _confirmedDeliveryTags)
			{
				if (!this.c_unconfirmedMessages.TryRemove(_confirmedDeliveryTag, out _removedMessage))
				{
					throw new ApplicationException("Could not remove delivery tag entry");
				}
			}
		}


		private void OnChannelNack(
			IModel channel,
			BasicNackEventArgs args)
		{
			// Todo
		}
	}
}
