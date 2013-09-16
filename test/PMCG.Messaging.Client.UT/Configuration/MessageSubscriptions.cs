using NUnit.Framework;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessageSubscriptions
	{
		private PMCG.Messaging.Client.Configuration.MessageSubscriptions c_SUT;


		[Test]
		public void Ctor_Where_Empty_Seed_Results_In_Empty_Collection()
		{
			this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageSubscriptions(
				new PMCG.Messaging.Client.Configuration.MessageSubscription[0]);

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(0, this.c_SUT.Configurations.Count());
		}


		[Test]
		public void Ctor_Where_Single_Item_Seed_Results_In_Single_Item_Collection()
		{
			this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageSubscriptions(
				new []
					{
						new  PMCG.Messaging.Client.Configuration.MessageSubscription(
							typeof(MyEvent),
							"TheQueueName",
							typeof(MyEvent).Name,
							message => SubscriptionHandlerResult.Completed)
					});

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(1, this.c_SUT.Configurations.Count());
			Assert.IsTrue(this.c_SUT.HasConfiguration(typeof(MyEvent).Name));
			Assert.IsNotNull(this.c_SUT[typeof(MyEvent).Name]);
			Assert.AreEqual(new[] { "TheQueueName" }, this.c_SUT.GetDistinctQueueNames().ToArray());
		}


		[Test, ExpectedException(typeof(ArgumentException))]
		public void Ctor_Where_Duplicate_Seed_Item_Type_Headers_Results_In_An_Exception()
		{
			this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageSubscriptions(
				new[]
					{
						new  PMCG.Messaging.Client.Configuration.MessageSubscription(
							typeof(MyEvent),
							"TheQueueName",
							"** DUPLICATE_TYPE_HEADER ***",
							message => SubscriptionHandlerResult.Completed),
						new  PMCG.Messaging.Client.Configuration.MessageSubscription(
							typeof(MyEvent),
							"TheQueueName",
							"** DUPLICATE_TYPE_HEADER ***",
							message => SubscriptionHandlerResult.Completed)
					});
		}


		[Test]
		public void Ctor_Where_Seed_Has_Same_Message_Types_But_Using_Different_Type_Headers_Results_In_An_Collection_With_Two_Items()
		{
			this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageSubscriptions(
				new[]
					{
						new  PMCG.Messaging.Client.Configuration.MessageSubscription(
							typeof(MyEvent),
							"Q_1",
							"TYPE_HEADER_1",
							message => SubscriptionHandlerResult.Completed),
						new  PMCG.Messaging.Client.Configuration.MessageSubscription(
							typeof(MyEvent),
							"Q_2",
							"TYPE_HEADER_2",
							message => SubscriptionHandlerResult.Completed)
					});

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(2, this.c_SUT.Configurations.Count());
			Assert.IsTrue(this.c_SUT.HasConfiguration("TYPE_HEADER_1"));
			Assert.IsNotNull(this.c_SUT["TYPE_HEADER_1"]);
			Assert.IsTrue(this.c_SUT.HasConfiguration("TYPE_HEADER_2"));
			Assert.IsNotNull(this.c_SUT["TYPE_HEADER_2"]);
			Assert.AreEqual(new[] { "Q_1", "Q_2" }, this.c_SUT.GetDistinctQueueNames().ToArray());
		}
	}
}
