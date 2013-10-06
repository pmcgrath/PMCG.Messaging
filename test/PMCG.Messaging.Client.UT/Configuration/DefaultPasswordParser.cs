using NUnit.Framework;
using System;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class DefaultPasswordParser
	{
		private PMCG.Messaging.Client.Configuration.DefaultPasswordParser c_SUT = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser();


		[Test]
		public void Parse_Where_Entry_For_This_Machine_Exists_Results_In_Parse_Result()
		{
			var _password = "ThePassword";
			var _passwordCipher = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser().Encrypt(_password);
			var _passwordSetting = string.Format("{0}:{1}", Environment.MachineName, _passwordCipher);

			var _result = this.c_SUT.Parse(_passwordSetting);

			Assert.IsNotNull(_password, _result);
		}


		[Test]
		public void Parse_Where_Multiple_Entries_Including_One_For_This_Machine_Exists_Results_In_Parse_Result()
		{
			var _thisMachinesPassword = "kjfhf%£$£$%%%!!!&iii9977";
			var _otherPassword = "ThePassword1";
			var _thisMachinesPasswordCipher = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser().Encrypt(_thisMachinesPassword);
			var _otherPasswordCipher = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser().Encrypt(_otherPassword);

			var _passwordSetting = string.Format("Machine1:{0},{1}:{2},Machine3:{0}", _otherPasswordCipher, Environment.MachineName, _thisMachinesPasswordCipher);

			var _result = this.c_SUT.Parse(_passwordSetting);

			Assert.IsNotNull(_thisMachinesPassword, _result);
		}


		[Test, ExpectedException(typeof(ApplicationException))]
		public void Parse_Where_No_Entry_For_This_Machine_Exists_Results_In_An_Exception()
		{
			this.c_SUT.Parse("Machine1:Pwd1,Machine2:OtherPwd");
		}
	}
}
