using System;


namespace PMCG.Messaging.Client.Configuration
{
	public interface IPasswordParser
	{
		string Parse(
			string source);
	}
}