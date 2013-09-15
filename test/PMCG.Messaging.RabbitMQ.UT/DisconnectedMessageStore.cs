using NUnit.Framework;
using PMCG.Messaging.RabbitMQ;
using System;
using System.IO;
using System.Linq;


namespace PMCG.Messaging.RabbitMQ.UT
{
	[TestFixture]
	public class DisconnectedMessageStore
	{
		private string c_testDirectoryPath;
		private PMCG.Messaging.RabbitMQ.DisconnectedMessageStore c_SUT;


		[SetUp]
		public void SetUp()
		{
			this.c_testDirectoryPath = string.Format(@"d:\temp\{0}", Guid.NewGuid());
			Directory.CreateDirectory(this.c_testDirectoryPath);

			this.c_SUT = new PMCG.Messaging.RabbitMQ.DisconnectedMessageStore(this.c_testDirectoryPath);
		}


		[TearDown]
		public void Teardown()
		{
			Directory.Delete(this.c_testDirectoryPath, true);
		}


		[Test]
		public void Store_Where_Two_Messages_Stored_Results_In_Two_Keys()
		{
			this.c_SUT.Store(
				new MyEvent(Guid.NewGuid(), ".", 1),
				new MyEvent(Guid.NewGuid(), ".", 2));

			var _messageKeysCount = this.c_SUT.GetAllMessageKeys().Count();

			Assert.AreEqual(2, _messageKeysCount);
		}

	
		[Test]
		public void Write_Message_Results_In_File_Creation()
		{
			var _originalMessage = new MyEvent(Guid.NewGuid(), ".", 12);

			var _messageKey = this.c_SUT.WriteMessage(_originalMessage);

			Assert.IsTrue(File.Exists(_messageKey));
		}


		[Test]
		public void GetAllMessageKeys_Where_No_Messages_Exist_Results_In_No_Keys()
		{
			Assert.AreEqual(0, this.c_SUT .GetAllMessageKeys().Count());
		}


		[Test]
		public void GetAllMessageKeys_Where_Two_Messages_Exist_Results_In_Two_Keys()
		{
			var _messageKey1 = this.c_SUT.WriteMessage(new MyEvent(Guid.NewGuid(), ".", 1));
			var _messageKey2 = this.c_SUT.WriteMessage(new MyEvent(Guid.NewGuid(), ".", 2));

			var _messageKeys = this.c_SUT.GetAllMessageKeys().ToArray();

			Assert.AreEqual(2, _messageKeys.Length);
			Assert.AreEqual(_messageKey1, _messageKeys[0]);
			Assert.AreEqual(_messageKey2, _messageKeys[1]);
		}


		[Test]
		public void RoundTrip_Results_In_Same_Message()
		{
			var _originalMessage = new MyEvent(Guid.NewGuid(), ".", 12);

			var _messageKey = this.c_SUT.WriteMessage(_originalMessage);
			var _roundTrippedMessage = this.c_SUT.ReadMessage(_messageKey) as MyEvent;

			Assert.IsNotNull(_roundTrippedMessage);
			Assert.AreEqual(_originalMessage.Id, _roundTrippedMessage.Id);
			Assert.AreEqual(_originalMessage.Detail, _roundTrippedMessage.Detail);
			Assert.AreEqual(_originalMessage.Number, _roundTrippedMessage.Number);
		}


		[Test]
		public void RemoveMessage_Where_We_Write_Message_And_Then_Delete_Results_In_No_Message_Keys()
		{
			var _messageKey = this.c_SUT.WriteMessage(new MyEvent(Guid.NewGuid(), ".", 12));
			this.c_SUT.RemoveMessage(_messageKey);

			var _messageCount = this.c_SUT.GetAllMessageKeys().Count();

			Assert.AreEqual(0, _messageCount);
		}


		[Test]
		public void GetMessageIdFromKey_Where_Results_In_Matched_Message_Id()
		{
			var _message = new MyEvent(Guid.NewGuid(), ".", 1);
			var _messageKey = this.c_SUT.WriteMessage(_message);

			var _messageId = this.c_SUT.GetMessageIdFromKey(_messageKey);

			Assert.AreEqual(_message.Id, _messageId);
		}
	}
}
