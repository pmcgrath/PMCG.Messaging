using System;


namespace PMCG.Messaging.Client
{
	public class PublisherResult
	{
		public readonly QueuedMessage QueuedMessage;
		public readonly PublisherResultStatus Status;
		public readonly string StatusContext;


		public PublisherResult(
			QueuedMessage queuedMessage,
			PublisherResultStatus status,
			string statusContext = null)
		{
			this.QueuedMessage = queuedMessage;
			this.Status = status;
			this.StatusContext = statusContext;
		}
	}
}