using NUnit.Framework;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class ConnectionStringSettingsParser
	{
		private PMCG.Messaging.Client.Configuration.ConnectionStringSettingsParser c_SUT = new PMCG.Messaging.Client.Configuration.ConnectionStringSettingsParser();


		[Test, ExpectedException]
		public void Parse_Where_Null_String_Results_In_An_Exception()
		{
			this.c_SUT.Parse(null);
		}


		[Test, ExpectedException]
		public void Parse_Where_Empty_String_Results_In_An_Exception()
		{
			this.c_SUT.Parse(" ");
		}


		[Test]
		public void Parse_Where_Single_Host_Results_In_A_Single_Connection_String()
		{
			var _result = this.c_SUT.Parse("hosts=localhost;port=5672;virtualhost=/;username=guest;ispasswordencrypted=false;password=Pass");

			Assert.IsNotNull(_result);
			Assert.AreEqual(1, _result.Count());
			Assert.AreEqual("amqp://guest:Pass@localhost:5672/", _result.First());
		}


		[Test]
		public void Parse_Where_Multiple_Hosts_Results_In_A_Multiple_Connection_Strings()
		{
			var _result = this.c_SUT.Parse("hosts=host1,host2;port=5672;virtualhost=/;username=guest;password=thepass");

			Assert.IsNotNull(_result);
			Assert.AreEqual(2, _result.Count());
			Assert.AreEqual("amqp://guest:thepass@host1:5672/", _result.First());
			Assert.AreEqual("amqp://guest:thepass@host2:5672/", _result.Skip(1).First());
		}


		[Test]
		public void Parse_Where_Single_Host_With_Encrypted_Password_Results_In_A_Single_Connection_String()
		{
			var _passwordCipher = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser().Encrypt("ThePassword");
			var _connectionStringSettings = string.Format("hosts=localhost;port=5672;virtualhost=/;username=guest;ispasswordencrypted=true;password={0}:{1}", Environment.MachineName, _passwordCipher);

			var _result = this.c_SUT.Parse(_connectionStringSettings);

			Assert.IsNotNull(_result);
			Assert.AreEqual(1, _result.Count());
			Assert.AreEqual("amqp://guest:ThePassword@localhost:5672/", _result.First());
		}


		[Test]
		public void Parse_Where_Multiple_Hosts_With_Encryped_Passwords_Results_In_A_Multiple_Connection_Strings()
		{
			var _passwordCipher = new PMCG.Messaging.Client.Configuration.DefaultPasswordParser().Encrypt("ThePassword");
			var _connectionStringSettings = string.Format("hosts=host1,host2;port=5;virtualhost=/dev;username=ted;ispasswordencrypted=true;password=M1:encryptedpassword,{0}:{1}", Environment.MachineName, _passwordCipher);

			var _result = this.c_SUT.Parse(_connectionStringSettings);

			Assert.IsNotNull(_result);
			Assert.AreEqual(2, _result.Count());
			Assert.AreEqual("amqp://ted:ThePassword@host1:5/dev", _result.First());
			Assert.AreEqual("amqp://ted:ThePassword@host2:5/dev", _result.Skip(1).First());
		}


		[Test]
		public void Parse_Where_Only_Hosts_Specified_Results_In_A_Multiple_Connection_Strings()
		{
			var _result = this.c_SUT.Parse("hosts=host1,host2");

			Assert.IsNotNull(_result);
			Assert.AreEqual(2, _result.Count());
			Assert.AreEqual("amqp://guest:guest@host1:5672/", _result.First());
			Assert.AreEqual("amqp://guest:guest@host2:5672/", _result.Skip(1).First());
		}
	}
}
