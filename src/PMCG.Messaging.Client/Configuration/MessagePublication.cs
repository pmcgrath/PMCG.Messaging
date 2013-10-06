using System;
using System.Collections.Generic;
using System.Linq;



namespace PMCG.Messaging.Client.Configuration
{
	public class MessagePublication
	{
		public readonly Type Type;
		public readonly IEnumerable<MessageDelivery> Configurations;


		public MessagePublication(
			Type type,
			IEnumerable<MessageDelivery> configurations)
		{
			Check.RequireArgumentNotNull("type", type);
			Check.RequireArgument("type", type, type.IsSubclassOf(typeof(Message)));
			Check.RequireArgumentNotNull("configurations", configurations);

			this.Type = type;
			this.Configurations = configurations.ToArray();
		}
	}
}