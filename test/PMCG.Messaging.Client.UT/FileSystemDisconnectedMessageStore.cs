using NUnit.Framework;
using PMCG.Messaging.Client;
using System;
using System.IO;
using System.Linq;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class FileSystemDisconnectedMessageStore
	{
		private string c_testDirectoryPath;
		private PMCG.Messaging.Client.FileSystemDisconnectedMessageStore c_SUT;


		[SetUp]
		public void SetUp()
		{
			this.c_testDirectoryPath = string.Format(@"d:\temp\{0}", Guid.NewGuid());
			Directory.CreateDirectory(this.c_testDirectoryPath);

			this.c_SUT = new PMCG.Messaging.Client.FileSystemDisconnectedMessageStore(this.c_testDirectoryPath);
		}


		[TearDown]
		public void Teardown()
		{
			Directory.Delete(this.c_testDirectoryPath, true);
		}


		[Test]
		public void Add_Message_Results_In_A_Single_Key()
		{
			this.c_SUT.Add(new MyEvent(Guid.NewGuid(), ".", 12));

			Assert.AreEqual(1, Directory.GetFiles(this.c_testDirectoryPath).Length);
		}


		[Test]
		public void Add_Two_Messages_Results_In_Two_Keys()
		{
			this.c_SUT.Add(new MyEvent(Guid.NewGuid(), ".", 1));
			this.c_SUT.Add(new MyEvent(Guid.NewGuid(), ".", 2));

			Assert.AreEqual(2, Directory.GetFiles(this.c_testDirectoryPath).Length);
		}


		[Test]
		public void GetAllIds_Where_No_Messages_Exist_Results_In_Empty_Collection()
		{
			Assert.AreEqual(0, this.c_SUT.GetAllIds().Count());
		}


		[Test]
		public void GetAllIds_Where_Two_Messages_Exist_Results_In_A_Collection_With_The_Two_Ids()
		{
			var _message1 = new MyEvent(Guid.NewGuid(), ".", 1);
			this.c_SUT.Add(_message1);

			var _message2 = new MyEvent(Guid.NewGuid(), ".", 2);
			this.c_SUT.Add(_message2);

			var _messageIds = this.c_SUT.GetAllIds().ToArray();

			Assert.AreEqual(2, _messageIds.Length);
			Assert.AreEqual(_message1.Id, _messageIds[0]);
			Assert.AreEqual(_message2.Id, _messageIds[1]);
		}


		[Test]
		public void RoundTrip_Results_In_Same_Message()
		{
			var _originalMessage = new MyEvent(Guid.NewGuid(), ".", 12);

			this.c_SUT.Add(_originalMessage);
			var _roundTrippedMessage = this.c_SUT.Get(_originalMessage.Id) as MyEvent;

			Assert.IsNotNull(_roundTrippedMessage);
			Assert.AreEqual(_originalMessage.Id, _roundTrippedMessage.Id);
			Assert.AreEqual(_originalMessage.Detail, _roundTrippedMessage.Detail);
			Assert.AreEqual(_originalMessage.Number, _roundTrippedMessage.Number);
		}


		[Test]
		public void Delete_Where_We_Write_Message_And_Then_Delete_Results_In_No_Message_Ids()
		{
			var _message = new MyEvent(Guid.NewGuid(), ".", 12);
			this.c_SUT.Add(_message);

			this.c_SUT.Delete(_message.Id);

			var _messageCount = this.c_SUT.GetAllIds().Count();

			Assert.AreEqual(0, _messageCount);
		}
	}
}
