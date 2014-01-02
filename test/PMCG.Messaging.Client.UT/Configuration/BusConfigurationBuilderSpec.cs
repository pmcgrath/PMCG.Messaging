using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class BusConfigurationBuilderSpec
	{
		private BusConfigurationBuilder c_SUT;


		[SetUp]
		public void SetUp()
		{
			this.c_SUT = new BusConfigurationBuilder();
		}


		[Test]
		public void RegisterPublication_Where_Event_Is_To_Be_Published_To_Multiple_Exchanges_Results_Multiple_Publication_Configurations()
		{
			this.c_SUT
				.RegisterPublication<MyEvent>("AnExchange1")
				.RegisterPublication<MyEvent>("AnExchange2");

			Assert.AreEqual(2, this.c_SUT.MessagePublications[typeof(MyEvent)].Count());
		}


		[Test]
		public void RegisterPublication_Where_First_Command_Instance_Being_Resistered_Results_In_An_Single_Publication_Entry()
		{
			this.c_SUT.RegisterPublication<MyCommand>("AnExchange1");

			Assert.AreEqual(1, this.c_SUT.MessagePublications[typeof(MyCommand)].Count());
		}

	
		[Test, ExpectedException]
		public void RegisterPublication_Where_Command_Already_Registered_And_Attempting_To_Register_Second_Command_Publication_Results_In_An_Exception()
		{
			this.c_SUT
				.RegisterPublication<MyCommand>("AnExchange1")
				.RegisterPublication<MyCommand>("AnExchange2");
		}
	}
}
