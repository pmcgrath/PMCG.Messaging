using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class MessageConsumers : IEnumerable<MessageConsumer>
	{
		private readonly MessageConsumer[] c_items;


		public MessageConsumer this[
			string typeHeader]
		{
			get
			{
				return this.c_items.FirstOrDefault(item => item.TypeHeader == typeHeader);
			}
		}


		public MessageConsumers(
			IEnumerable<MessageConsumer> items)
		{
			Check.RequireArgumentNotNull("items", items);
			Check.RequireArgument("items", items, items.Count() == items.Select(item => item.TypeHeader).Distinct().Count());

			this.c_items = items.ToArray();
		}


		public IEnumerator<MessageConsumer> GetEnumerator()
		{
			return this.c_items.AsEnumerable().GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}


		public bool HasConfiguration(
			string typeHeader)
		{
			return this.c_items.Any(item => item.TypeHeader == typeHeader);
		}


		public IEnumerable<string> GetDistinctQueueNames()
		{
			return this.c_items.Select(item => item.QueueName).Distinct();
		}


		public IEnumerable<MessageConsumer> GetTransientQueueConfigurations()
		{
			return this.c_items.Where(item => item.UseTransientQueue);
		}
	}
}