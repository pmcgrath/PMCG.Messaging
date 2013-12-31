using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Publication
	{
		private readonly MessageDelivery c_configuration;
		private readonly Message c_message;
		private readonly TaskCompletionSource<PublicationResult> c_result;


		public string Id { get { return this.c_message.Id.ToString(); } }
		public string CorrelationId { get { return this.c_message.CorrelationId; } }
		public string ExchangeName { get { return this.c_configuration.ExchangeName; } }
		public Byte DeliveryMode { get { return (byte)this.c_configuration.DeliveryMode; } }
		public string RoutingKey { get { return this.c_configuration.RoutingKeyFunc(this.c_message); } }
		public string TypeHeader { get { return this.c_configuration.TypeHeader; } }
		public Task<PublicationResult> ResultTask { get { return this.c_result.Task; } }
		public Message Message { get { return this.c_message; } }


		public Publication(
			MessageDelivery configuration,
			Message message,
			TaskCompletionSource<PublicationResult> result)
		{
			this.c_configuration = configuration;
			this.c_message = message;
			this.c_result = result;
		}


		public void SetResult(
			PublicationResultStatus status,
			string context = null)
		{
			var _publicationResult = new PublicationResult(this.c_configuration, this.c_message, status, context);
			this.c_result.SetResult(_publicationResult);
		}
	}
}