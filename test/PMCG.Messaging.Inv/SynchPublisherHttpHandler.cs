using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Web;


namespace PMCG.Messaging.Inv
{
	/* Configure ass follows in a web.config
		<configuration>
			<system.web>
				<httpHandlers>
					<add verb="*" path="publisher"  type="PublisherHttpHandler, PMCG.Messaging.Inv" />
				</httpHandlers>
			<system.web>
		</configuration>
	*/
	public class PublisherHttpHandler : IHttpHandler
	{
		private static readonly string c_connectionUri = Configuration.LocalConnectionUri;
		private static readonly string c_exchangeName = "test_publisher_confirms";
		private static readonly IConnection c_connection;


		public bool IsReusable { get { return false; } }


		static PublisherHttpHandler()
		{
			var _connectionFactory = new ConnectionFactory { Uri = PublisherHttpHandler.c_connectionUri };
			PublisherHttpHandler.c_connection = _connectionFactory.CreateConnection();
		}
		

		public void ProcessRequest(
			HttpContext context)
		{
			// Toggle to send message
			var _doSendAMessage = context.Request.QueryString["sendamessage"] == "true";
			var _messageBodyContent = string.Empty;
			var _stopwatch = Stopwatch.StartNew();
			if (_doSendAMessage)
			{
				using (var _channel = PublisherHttpHandler.c_connection.CreateModel())
				{
					_channel.ConfirmSelect();

					var _properties = _channel.CreateBasicProperties();
					_properties.ContentType = "text/plain";
					_properties.DeliveryMode = 2;
					_properties.MessageId = Guid.NewGuid().ToString();

					_messageBodyContent = string.Format("Message published @ {0} with Id {1}", DateTime.Now, _properties.MessageId);
					var _messageBody = Encoding.UTF8.GetBytes(_messageBodyContent);

					_channel.BasicPublish(PublisherHttpHandler.c_exchangeName, string.Empty, _properties, _messageBody);
					_channel.WaitForConfirms();
					
				}
			}
			_stopwatch.Stop();
			context.Response.Write(string.Format("<html><body>Completed in <i>{0}</i> milliseconds, send message = {1}, content = [{2}]</body></html>", _doSendAMessage, _messageBodyContent, _stopwatch.ElapsedMilliseconds));
		}
	}
}
