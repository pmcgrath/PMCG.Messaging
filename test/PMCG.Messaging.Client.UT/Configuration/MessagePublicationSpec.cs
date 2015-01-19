using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessagePublicationSpec
	{
		[Test]
		public void Ctor_Where_Event_With_Multiple_Message_Deliveries_Results_In_Object_Creation()
		{
			var _SUT = new MessagePublication(
				typeof(MyEvent),
				new []
					{
						new MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty),
						new MessageDelivery(
							"exchangeName2",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					});

			Assert.AreEqual(2, _SUT.Configurations.Count());
		}


		[Test]
		public void Ctor_Where_Command_With_Single_Message_Delivery_Results_In_Object_Creation()
		{
			var _SUT = new MessagePublication(
				typeof(MyCommand),
				new []
					{
						new MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					});

			Assert.AreEqual(1, _SUT.Configurations.Count());
		}


		[Test, ExpectedException]
		public void Ctor_Where_Command_With_Multiple_Message_Deliveries_Results_In_An_Exception()
		{
			new MessagePublication(
				typeof(MyCommand),
				new []
					{
						new MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty),
						new MessageDelivery(
							"exchangeName2",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					});
		}
	}
}
