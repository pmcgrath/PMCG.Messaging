using NUnit.Framework;
using System;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class ConnectionManager
	{
		[Test]
		public void Open_Where_Only_Allowed_One_Attempt_But_Using_Wrong_Port_Number_Results_In_Connection_Not_Being_Opened()
		{
			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				new [] { TestingConfiguration.LocalConnectionUri.Replace("5672", "25672") },
				TimeSpan.FromSeconds(5));
			_SUT.Open(1);

			Assert.IsFalse(_SUT.IsOpen);
		}


		[Test, ExpectedException]
		public void Open_Where_Already_Opened_And_Second_Open_Call_Made_Results_In_Connection_Not_Being_Opened()
		{
			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				new[] { TestingConfiguration.LocalConnectionUri },
				TimeSpan.FromSeconds(5));
			_SUT.Open();

			_SUT.Open();
		}
	}
}
