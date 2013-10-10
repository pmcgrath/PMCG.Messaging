using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class MessageConsumers
	{
		public readonly IEnumerable<MessageConsumer> Configurations;


		public MessageConsumer this[
			string typeHeader]
		{
			get
			{
				return this.Configurations.FirstOrDefault(configuration => configuration.TypeHeader == typeHeader);
			}
		}


		public MessageConsumers(
			IEnumerable<MessageConsumer> configurations)
		{
			Check.RequireArgumentNotNull("configurations", configurations);
			Check.RequireArgument("configurations", configurations, configurations.Count() == 
				configurations.Select(configuration => configuration.TypeHeader).Distinct().Count());

			this.Configurations = configurations.ToArray();
		}


		public bool HasConfiguration(
			string typeHeader)
		{
			return this.Configurations.Any(configuration => configuration.TypeHeader == typeHeader);
		}


		public IEnumerable<string> GetDistinctQueueNames()
		{
			return this.Configurations.Select(configuration => configuration.QueueName).Distinct();
		}


		public IEnumerable<MessageConsumer> GetTransientQueueConfigurations()
		{
			return this.Configurations.Where(configuration => configuration.UseTransientQueue);
		}
	}
}