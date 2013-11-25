using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Publisher
	{
		[Test, ExpectedException]
		public void PublishAsync_Where_Cancellation_Token_Is_Cancelled_Results_In_An_Exception()
		{
			var _connection = Substitute.For<IConnection>();
			var _cancellationTokenSource = new CancellationTokenSource();

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _cancellationTokenSource.Token);
			_cancellationTokenSource.Cancel();

			_SUT.PublishAsync(_queuedMessage);
		}


		[Test, ExpectedException]
		public void PublishAsync_Where_Channel_Is_Closed_Results_In_An_Exception()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);

			_SUT.PublishAsync(_queuedMessage);
		}


		[Test, ExpectedException]
		public void PublishAsync_Where_Channel_Publication_Fails_Results_In_An_Exception()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => { throw new ApplicationException("Bang !"); } );

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);

			_SUT.PublishAsync(_queuedMessage);
		}


		[Test]
		public void PublishAsync_Where_Single_Message_Published_And_Acked_Results_In_Task_Completion()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _messageProperties = Substitute.For<IBasicProperties>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);
			_channel.CreateBasicProperties().Returns(_messageProperties);

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);
			_channel.NextPublishSeqNo.Returns(1UL);
			
			var _task = _SUT.PublishAsync(_queuedMessage);

			Assert.IsFalse(_task.IsCompleted);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
			Assert.IsTrue(_task.IsCompleted);

			_messageProperties.Received().ContentType = "application/json";
			_messageProperties.Received().DeliveryMode = (byte)_messageDelivery.DeliveryMode;
			_messageProperties.Received().Type = _messageDelivery.TypeHeader;
			_messageProperties.Received().MessageId = _myEvent.Id.ToString();
			_messageProperties.Received().CorrelationId = _myEvent.CorrelationId;
			_channel.Received().BasicPublish(_messageDelivery.ExchangeName, _messageDelivery.RoutingKeyFunc(_myEvent), _messageProperties, Arg.Any<byte[]>());
		}


		[Test]
		public void PublishAsync_Where_Multiple_Messages_Published_And_A_Single_Ack_Results_In_Completed_Tasks()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);

			var _tasks = new List<Task<PublisherResult>>();
			var _numberOfMessagesToPublish = 10;
			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);
			for (var _index = 1; _index <= 10; _index++)
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

				_channel.NextPublishSeqNo.Returns((ulong)_index);
				var _task = _SUT.PublishAsync(_queuedMessage);
				_tasks.Add(_task);
			}

			foreach(var _task in _tasks) { Assert.IsFalse(_task.IsCompleted); }
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_numberOfMessagesToPublish });
			foreach(var _task in _tasks)
			{
				Assert.IsTrue(_task.IsCompleted);
				Assert.AreEqual(PublisherResultStatus.Acked, _task.Result.Status);
			}
		}


		[Test]
		public void PublishAsync_Where_100_Messages_Published_And_A_Single_Multi_Ack_For_Some_Messages_Results_In_Some_Completed_Tasks_And_Some_Still_Pending()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);

			var _numberOfMessagesToPublish = 100;
			var _tasks = new Task<PublisherResult>[_numberOfMessagesToPublish];
			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);
			for (var _index = 0; _index < _numberOfMessagesToPublish; _index++)
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

				_channel.NextPublishSeqNo.Returns((ulong)(_index + 1));
				var _task = _SUT.PublishAsync(_queuedMessage);
				_tasks[_index] = _task;
			}

			foreach (var _task in _tasks) { Assert.IsFalse(_task.IsCompleted); }
			
			var _deliveryTagToAcknowledge = 73;
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_deliveryTagToAcknowledge });
			
			for (var _index = 0; _index < _deliveryTagToAcknowledge; _index++)
			{
				Assert.IsTrue(_tasks[_index].IsCompleted);
				Assert.AreEqual(PublisherResultStatus.Acked, _tasks[_index].Result.Status);
			}
			for (var _index = _deliveryTagToAcknowledge; _index < _numberOfMessagesToPublish; _index++)
			{
				Assert.IsFalse(_tasks[_index].IsCompleted);
			}
		}


		[Test]
		public void PublishAsync_Where_Single_Message_Published_And_Nacked_Results_In_Nacked_Task_Result()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);
			_channel.NextPublishSeqNo.Returns(1UL);
			
			var _task = _SUT.PublishAsync(_queuedMessage);

			Assert.IsFalse(_task.IsCompleted);
			_channel.BasicNacks += Raise.Event<BasicNackEventHandler>(_channel, new BasicNackEventArgs { Multiple = false, DeliveryTag = 1 });
			Assert.IsTrue(_task.IsCompleted);
			Assert.AreEqual(PublisherResultStatus.Nacked, _task.Result.Status);
			Assert.IsNull(_task.Result.StatusContext);
		}


		[Test]
		public void PublishAsync_Where_Two_Messages_Being_Published_But_Channel_Is_Closed_Before_Acks_Received_Results_In_Two_Channel_Shutdown_Tasks()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);

			_channel.NextPublishSeqNo.Returns(1UL);
			var _task1 = _SUT.PublishAsync(_queuedMessage);
			_channel.NextPublishSeqNo.Returns(2UL);
			var _task2 = _SUT.PublishAsync(_queuedMessage);

			_channel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Bang!"));

			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_task1.IsCompleted);
			Assert.AreEqual(PublisherResultStatus.ChannelShutdown, _task1.Result.Status);
			Assert.IsTrue(_task1.Result.StatusContext.Contains("Bang!"));
			Assert.IsTrue(_task2.IsCompleted);
			Assert.AreEqual(PublisherResultStatus.ChannelShutdown, _task2.Result.Status);
			Assert.IsTrue(_task2.Result.StatusContext.Contains("Bang!"));
		}


		[Test]
		public void PublishAsync_Where_Exchange_Does_Not_Exist_Results_In_Channel_Shutdown_And_A_Channel_Shutdown_Task_Result()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();

			_connection.CreateModel().Returns(_channel);
			_channel.IsOpen.Returns(true);

			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _messageDelivery = new MessageDelivery("NON_EXISTENT_EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _queuedMessage = new QueuedMessage(_messageDelivery, _myEvent);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, CancellationToken.None);

			_channel.NextPublishSeqNo.Returns(1UL);
			var _task = _SUT.PublishAsync(_queuedMessage);

			_channel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "404 Exchange does not exist !"));
			 
			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_task.IsCompleted);
			Assert.AreEqual(PublisherResultStatus.ChannelShutdown, _task.Result.Status);
			Assert.IsTrue(_task.Result.StatusContext.Contains("404 Exchange does not exist !"));
		}
	}
}
