using PMCG.Messaging.RabbitMQ.Utility;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.RabbitMQ.Configuration
{
	public class MessageSubscriptions
	{
		public readonly ushort PrefetchCount;
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
			ushort prefetchCount,
			IEnumerable<MessageSubscription> configurations)
		{
			Check.RequireArgument("prefetchCount", prefetchCount, prefetchCount > 0);
			Check.RequireArgumentNotNull("configurations", configurations);
			Check.RequireArgument("configurations", configurations, configurations.Count() == 
				configurations.Select(configuration => configuration.TypeHeader).Distinct().Count());

			this.PrefetchCount = prefetchCount;
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