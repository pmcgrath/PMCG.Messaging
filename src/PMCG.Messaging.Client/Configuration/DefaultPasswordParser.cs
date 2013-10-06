using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace PMCG.Messaging.Client.Configuration
{
	public class DefaultPasswordParser : IPasswordParser
	{
		public string Parse(
			string source)
		{
			var _thisMachineEntryPrefix = string.Format("{0}:", Environment.MachineName);
			var _thisMachineEntry = source.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(item => item.StartsWith(_thisMachineEntryPrefix));
			if (_thisMachineEntry == null) { throw new ApplicationException("No entry found for current machine"); }

			var _cipher = _thisMachineEntry.Substring(_thisMachineEntryPrefix.Length);

			return this.Decrypt(_cipher);
		}


		public string Encrypt(
			string text)
		{
			return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(text), null, DataProtectionScope.CurrentUser));
		}


		public string Decrypt(
			string cipher)
		{
			return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(cipher), null, DataProtectionScope.CurrentUser));
		}
	}
}