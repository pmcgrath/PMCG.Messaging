using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


namespace PMCG.Messaging.Inv
{
	public class TopicsUsage
	{
		private readonly string c_connectionUri = "amqp://guest:guest@localhost:5672/";
		private readonly string c_logExchangeName = "test_topic_logs";
		private readonly string c_allLogsQueueName = "test_topic_logs";
		private readonly string c_app1LogQueueName = "test_topic_logs_app1";
		private readonly string c_errorLogQueueName = "test_topic_logs_error";
		private readonly Action<string> c_writeLog = message => Console.WriteLine("{0:hh:mm:ss.ffff} {1,3} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, message);


		private IConnection c_connection;


		public void Run()
		{
			this.c_writeLog("Opening connection");
			this.OpenConnection();

			this.c_writeLog("Ensuring configuration exists");
			this.EnsureConfigurationExists();

			this.c_writeLog("Running all logs subscriber");
			this.RunSubscriber(this.c_allLogsQueueName);

			this.c_writeLog("Running app1 log subscriber");
			this.RunSubscriber(this.c_app1LogQueueName);

			this.c_writeLog("Running error log subscriber");
			this.RunSubscriber(this.c_errorLogQueueName);

			this.c_writeLog("Running publisher");
			this.RunPublisher();

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

			_channel.ExchangeDeclare(this.c_logExchangeName, ExchangeType.Topic, false, false, null);

			_channel.QueueDeclare(this.c_allLogsQueueName, false, false, false, null);
			_channel.QueueBind(this.c_allLogsQueueName, this.c_logExchangeName, "#", null);

			_channel.QueueDeclare(this.c_app1LogQueueName, false, false, false, null);
			_channel.QueueBind(this.c_app1LogQueueName, this.c_logExchangeName, "app1.*", null);

			_channel.QueueDeclare(this.c_errorLogQueueName, false, false, false, null);
			_channel.QueueBind(this.c_errorLogQueueName, this.c_logExchangeName, "*.error", null);

			_channel.Close();
		}


		private void RunSubscriber(
			string queueName)
		{
			new Task(() =>
				{
					var _channel = this.c_connection.CreateModel();

					var _subscription = new Subscription(_channel, queueName);
					foreach (BasicDeliverEventArgs _messageDelivery in _subscription)
					{
						this.c_writeLog(string.Format("Received message on q {0}, tag = {1}", queueName, _messageDelivery.DeliveryTag));
						_subscription.Ack(_messageDelivery);
					}
				}).Start();
		}


		private void RunPublisher()
		{
			var _channel = this.c_connection.CreateModel();
			var _routingKey = "app1.info";
			do
			{
				var _properties = _channel.CreateBasicProperties();
				_properties.ContentType = "text/plain";
				_properties.DeliveryMode = 1;
				_properties.MessageId = Guid.NewGuid().ToString();

				var _messageBodyContent = string.Format("Message published @ {0} with routing key {1} and id {2}", DateTime.Now, _routingKey, _properties.MessageId);
				var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

				this.c_writeLog(string.Format("About to publish message ({0})", _messageBodyContent));
				_channel.BasicPublish(this.c_logExchangeName, _routingKey, _properties, _messageBody);

				Console.WriteLine();
				Console.WriteLine("Enter routing key i.e. app1.info : ");
			} while ((_routingKey = Console.ReadLine()) != "x");

			this.c_writeLog("Completed publishing");
		}


		private void CloseConnection()
		{
			this.c_connection.Close();
		}
	}
}
