using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessageConsumerSpec
	{
		private MessageConsumer c_SUT;


		[Test]
		public void Ctor_Where_Good_Params_Results_In_Object_Creation()
		{
			this.c_SUT = new MessageConsumer(
				typeof(MyEvent),
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message => ConsumerHandlerResult.Completed);

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(typeof(MyEvent), this.c_SUT.Type);
			Assert.AreEqual(TestingConfiguration.QueueName, this.c_SUT.QueueName);
			Assert.AreEqual(typeof (MyEvent).Name, this.c_SUT.TypeHeader);
		}


		[Test, ExpectedException(typeof(ArgumentException))]
		public void Ctor_Where_Type_Is_Not_A_Message_Results_In_An_Exception()
		{
			this.c_SUT = new MessageConsumer(
				this.GetType(),
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message => ConsumerHandlerResult.Completed);
		}
	}
}
