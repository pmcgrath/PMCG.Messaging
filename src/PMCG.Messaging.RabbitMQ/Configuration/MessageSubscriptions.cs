using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageSubscriptions
	{
		public readonly IEnumerable<MessageSubscription> Configurations;



		public MessageSubscription this[
			string typeHeader]
		{
			get
			{
				return this.Configurations.FirstOrDefault(configuration => configuration.TypeHeader == typeHeader);
			}
		}


		public MessageSubscriptions(
			IEnumerable<MessageSubscription> configurations)
		{
			Check.RequireArgumentNotNull("configurations", configurations);
			Check.RequireArgument("configurations", configurations, configurations.Count() == 
				configurations.Select(configuration => configuration.TypeHeader).Distinct().Count());

			this.Configurations = configurations.ToArray();
		}


		public IEnumerable<string> GetDistinctQueueNames()
		{
			return this.Configurations.Select(configuration => configuration.QueueName).Distinct();
		}


		public bool HasConfiguration(
			string typeHeader)
		{
			return this.Configurations.Any(configuration => configuration.TypeHeader == typeHeader);
		}
	}
}