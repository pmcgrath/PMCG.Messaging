using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Publisher
	{
		[Test, ExpectedException]
		public void Start_Where_Cancellation_Token_Is_Cancelled_Results_In_An_Exception()
		{
			var _connection = Substitute.For<IConnection>();
			var _cancellationTokenSource = new CancellationTokenSource();
			var _publicationQueue = new BlockingCollection<Publication>();

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, _cancellationTokenSource.Token);
			_cancellationTokenSource.Cancel();
			_SUT.Start();
		}


		[Test]
		public void Publish_Where_Channel_Is_Closed_Results_In_Faulted_Task()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel.CreateBasicProperties().Returns(callInfo => { throw new Exception("Channel not open !"); });

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();

			_publicationQueue.Add(_publication);
			Thread.Sleep(100);

			Assert.IsTrue(_publisherTask.IsFaulted);
		}


		[Test]
		public void Publish_Where_Channel_Publication_Fails_Results_In_A_Completed_Publisher_Task()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => { throw new ApplicationException("Bang !"); } );

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();

			_publicationQueue.Add(_publication);
			Thread.Sleep(100);

			Assert.IsTrue(_publisherTask.IsCompleted);
		}


		[Test]
		public void Publish_Where_Single_Publication_Published_And_Acked_Results_In_Task_Completion()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();
			var _messageProperties = Substitute.For<IBasicProperties>();

			_connection.CreateModel().Returns(_channel);
			_channel.CreateBasicProperties().Returns(_messageProperties);
			_channel.NextPublishSeqNo.Returns(1UL);

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			_publicationQueue.Add(_publication);
			Thread.Sleep(100);											// Allow publication to be read from queue

			Assert.IsFalse(_publication.ResultTask.IsCompleted);
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
			Assert.IsTrue(_publication.ResultTask.IsCompleted);

			_messageProperties.Received().ContentType = "application/json";
			_messageProperties.Received().DeliveryMode = (byte)_messageDelivery.DeliveryMode;
			_messageProperties.Received().Type = _messageDelivery.TypeHeader;
			_messageProperties.Received().MessageId = _myEvent.Id.ToString();
			_messageProperties.Received().CorrelationId = _myEvent.CorrelationId;
			_channel.Received().BasicPublish(_messageDelivery.ExchangeName, _messageDelivery.RoutingKeyFunc(_myEvent), _messageProperties, Arg.Any<byte[]>());
		}


		[Test]
		public void Publish_Where_Multiple_Publications_Published_And_A_Single_Ack_Results_In_Completed_Tasks()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			var _nextPublishSeqNo = 1UL;
			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 10; _index++)
			{
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

				_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}
			Thread.Sleep(100);											// Allow publications to be read from queue

			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_publications.Count });
			foreach(var _publication in _publications)
			{
				Assert.IsTrue(_publication.ResultTask.IsCompleted);
				Assert.AreEqual(PublicationResultStatus.Acked, _publication.ResultTask.Result.Status);
			}
		}


		[Test]
		public void Publish_Where_100_Messages_Published_And_A_Single_Multi_Ack_For_Some_Messages_Results_In_Some_Completed_Tasks_And_Some_Still_Pending()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			var _nextPublishSeqNo = 1UL;
			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 100; _index++)
			{
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

				_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}
			Thread.Sleep(100);											// Allow publications to be read from queue

			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			var _deliveryTagToAcknowledge = 73;
			_channel.BasicAcks += Raise.Event<BasicAckEventHandler>(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_deliveryTagToAcknowledge });

			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted), "A1");
			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted && publication.ResultTask.Result.Status == PublicationResultStatus.Acked), "A2");
			Assert.AreEqual(100 - _deliveryTagToAcknowledge, _publications.Count(publication => !publication.ResultTask.IsCompleted), "A3");
		}


		[Test]
		public void Publish_Where_Single_Message_Published_And_Nacked_Results_In_Nacked_Task_Result()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			_publicationQueue.Add(_publication);
			Thread.Sleep(100);											// Allow publications to be read from queue

			_channel.BasicNacks += Raise.Event<BasicNackEventHandler>(_channel, new BasicNackEventArgs { Multiple = true, DeliveryTag = 1UL });

			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.Nacked, _publication.ResultTask.Result.Status);
			Assert.IsNull(_publication.ResultTask.Result.StatusContext);
		}


		[Test]
		public void Publish_Where_Two_Messages_Being_Published_But_Channel_Is_Closed_Before_Acks_Received_Results_In_Two_Channel_Shutdown_Tasks()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul, 2UL);

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);

			var _taskCompletionSource1 = new TaskCompletionSource<PublicationResult>();
			var _publication1 = new Publication(_messageDelivery, _myEvent, _taskCompletionSource1);
			var _taskCompletionSource2 = new TaskCompletionSource<PublicationResult>();
			var _publication2 = new Publication(_messageDelivery, _myEvent, _taskCompletionSource2);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			_publicationQueue.Add(_publication1);
			_publicationQueue.Add(_publication2);
			Thread.Sleep(100);											// Allow publications to be read from queue

			_channel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Bang!"));

			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_publication1.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication1.ResultTask.Result.Status);
			Assert.IsTrue(_publication1.ResultTask.Result.StatusContext.Contains("Bang!"));
			Assert.IsTrue(_publication2.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication2.ResultTask.Result.Status);
			Assert.IsTrue(_publication2.ResultTask.Result.StatusContext.Contains("Bang!"));
		}


		[Test]
		public void Publish_Where_Exchange_Does_Not_Exist_Results_In_Channel_Shutdown_And_A_Channel_Shutdown_Task_Result()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul);

			var _messageDelivery = new MessageDelivery("NON_EXISTENT_EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue, CancellationToken.None);
			var _publisherTask = _SUT.Start();
			while (_publisherTask.Status != TaskStatus.Running) { }		// Spin till the task is running

			_publicationQueue.Add(_publication);
			Thread.Sleep(100);											// Allow publications to be read from queue

			_channel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "404 Exchange does not exist !"));
			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication.ResultTask.Result.Status);
			Assert.IsTrue(_publication.ResultTask.Result.StatusContext.Contains("404 Exchange does not exist !"));
		}
	}
}
