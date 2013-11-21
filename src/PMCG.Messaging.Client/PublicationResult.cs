using System;


namespace PMCG.Messaging.Client
{
	public class PublicationResult
	{
		public readonly QueuedMessage QueuedMessage;
		public readonly PublicationResultStatus Status;
		public readonly string StatusContext;


		public PublicationResult(
			QueuedMessage queuedMessage,
			PublicationResultStatus status,
			string statusContext = null)
		{
			this.QueuedMessage = queuedMessage;
			this.Status = status;
			this.StatusContext = statusContext;
		}
	}
}