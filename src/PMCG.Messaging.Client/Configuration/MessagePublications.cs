using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class MessagePublications : IEnumerable<MessagePublication>
	{
		private readonly MessagePublication[] c_items;


		public MessagePublication this[
			Type type]
		{
			get
			{
				return this.c_items.FirstOrDefault(item => item.Type == type);
			}
		}


		public MessagePublications(
			IEnumerable<MessagePublication> items)
		{
			Check.RequireArgumentNotNull("items", items);

			this.c_items = items.ToArray();
		}

		
		public IEnumerator<MessagePublication> GetEnumerator()
		{
			return this.c_items.AsEnumerable().GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}


		public bool HasConfiguration(
			Type type)
		{
			return this.c_items.Any(item => item.Type == type);
		}
	}
}