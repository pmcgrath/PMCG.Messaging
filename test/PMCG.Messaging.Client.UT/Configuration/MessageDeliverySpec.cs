using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessageDeliverySpec
	{
		[Test]
		public void Ctor_Cater_For_Direct_Exchange_Delivery()
		{
			var _SUT = new MessageDelivery(
				string.Empty,
				"TypeHeader",
				MessageDeliveryMode.Persistent,
				message => "DirectQueue");

			Assert.AreEqual(string.Empty, _SUT.ExchangeName);
			Assert.AreEqual("TypeHeader", _SUT.TypeHeader);
			Assert.AreEqual(MessageDeliveryMode.Persistent, _SUT.DeliveryMode);
			Assert.AreEqual("DirectQueue", _SUT.RoutingKeyFunc(null));
		}
	}
}
