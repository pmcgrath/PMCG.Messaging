using NUnit.Framework;
using System;


namespace PMCG.Messaging.RabbitMQ.UT.Configuration
{
	[TestFixture]
	public class MessageSubscription
	{
		private PMCG.Messaging.RabbitMQ.Configuration.MessageSubscription c_SUT;


		[Test]
		public void Ctor_Where_Good_Params_Results_In_Object_Creation()
		{
			this.c_SUT = new PMCG.Messaging.RabbitMQ.Configuration.MessageSubscription(
				typeof(MyEvent),
				"TheQueueName",
				typeof(MyEvent).Name,
				message => PMCG.Messaging.RabbitMQ.Configuration.MessageSubscriptionActionResult.Completed);

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(typeof(MyEvent), this.c_SUT.Type);
			Assert.AreEqual("TheQueueName", this.c_SUT.QueueName);
			Assert.AreEqual(typeof (MyEvent).Name, this.c_SUT.TypeHeader);
		}


		[Test, ExpectedException(typeof(ArgumentException))]
		public void Ctor_Where_Type_Is_Not_A_Message_Results_In_An_Exception()
		{
			this.c_SUT = new PMCG.Messaging.RabbitMQ.Configuration.MessageSubscription(
				this.GetType(),
				"TheQueueName",
				typeof(MyEvent).Name,
				message => PMCG.Messaging.RabbitMQ.Configuration.MessageSubscriptionActionResult.Completed);
		}
	}
}
