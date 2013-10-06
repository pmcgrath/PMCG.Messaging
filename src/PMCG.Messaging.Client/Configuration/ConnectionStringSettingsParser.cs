using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class ConnectionStringSettingsParser
	{
		private readonly IPasswordParser c_passwordParser;


		public ConnectionStringSettingsParser()
			: this(new DefaultPasswordParser())
		{
		}


		public ConnectionStringSettingsParser(
			IPasswordParser passwordParser)
		{
			Check.RequireArgumentNotNull("passwordParser", passwordParser);

			this.c_passwordParser = passwordParser;
		}


		public IEnumerable<string> Parse(
			string connectionStringSettings)
		{
			Check.RequireArgumentNotEmpty("connectionStringSettings", connectionStringSettings);

			var _settings = connectionStringSettings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var _hosts = this.GetSetting(_settings, "hosts", "localhost").Split(',');
			var _port = this.GetSetting(_settings, "port", "5672");
			var _virtualHost = this.GetSetting(_settings, "virtualhost", "/");
			var _userName = this.GetSetting(_settings, "username", "guest");
			var _isPasswordEncrypted = this.GetSetting(_settings, "ispasswordencrypted", "false");
			var _password = this.GetSetting(_settings, "password", "guest");

			if (bool.Parse(_isPasswordEncrypted)) { _password = this.c_passwordParser.Parse(_password); }

			var _connectionStringTemplate = "amqp://{0}:{1}@{2}:{3}{4}";
			return _hosts.Select(host => string.Format(_connectionStringTemplate, _userName, _password, host, _port, _virtualHost)).ToArray();
		}


		private string GetSetting(
			IEnumerable<string> settings,
			string key,
			string defaultValue)
		{
			var _keyPrefix = string.Format("{0}=", key);
			var _setting = settings.FirstOrDefault(setting => setting.StartsWith(_keyPrefix));
			return _setting != null ? _setting.Substring(_keyPrefix.Length) : defaultValue;
		}
	}
}