using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace PMCG.Messaging.Client.Configuration
{
	/*
		Can use the following powershell at each machine to generate the user's password on the machine (Will need to run as the specific user)
			# Required for cryptography types
			[System.Reflection.Assembly]::LoadWithPartialName("System.Security") | out-null;

			$text = 'ThePassword';
			$cipherText = [Convert]::ToBase64String([System.Security.Cryptography.ProtectedData]::Protect([System.Text.Encoding]::UTF8.GetBytes($text), $null, 'CurrentUser'));
			$decryptedText = [System.Text.Encoding]::UTF8.GetString([System.Security.Cryptography.ProtectedData]::Unprotect([Convert]::FromBase64String($cipherText), $null, 'CurrentUser'));

			write-host ("{0,-20}{1}" -f 'Text', $text);
			write-host ("{0,-20}{1}" -f 'Cipher', $cipherText);
			write-host ("{0,-20}{1}" -f 'Decrypted', $decryptedText);
		See http://msdn.microsoft.com/en-us/library/system.security.cryptography.protecteddata.aspx
			If the user's password is changed, this will prevent the decryption of the password generated from the above domain account
	*/
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