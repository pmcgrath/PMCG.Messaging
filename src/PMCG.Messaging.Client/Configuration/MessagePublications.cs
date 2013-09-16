using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessagePublications
	{
		public readonly IEnumerable<MessagePublication> Configurations;


		public MessagePublication this[
			Type type]
		{
			get
			{
				return this.Configurations.FirstOrDefault(configuration => configuration.Type == type);
			}
		}


		public MessagePublications(
			IEnumerable<MessagePublication> configurations)
		{
			Check.RequireArgumentNotNull("configurations", configurations);

			this.Configurations = configurations.ToArray();
		}


		public bool HasConfiguration(
			Type type)
		{
			return this.Configurations.Any(configuration => configuration.Type == type);
		}
	}
}