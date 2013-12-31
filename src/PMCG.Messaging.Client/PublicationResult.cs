using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client
{
	public class PublicationResult
	{
		public readonly MessageDelivery Configuration;
		public readonly Message Message;
		public readonly PublicationResultStatus Status;
		public readonly string StatusContext;


		public PublicationResult(
			MessageDelivery configuration,
			Message message,
			PublicationResultStatus status,
			string statusContext = null)
		{
			this.Configuration = configuration;
			this.Message = message;
			this.Status = status;
			this.StatusContext = statusContext;
		}
	}
}