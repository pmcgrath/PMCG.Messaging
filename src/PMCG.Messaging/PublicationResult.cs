using System;


namespace PMCG.Messaging
{
	public class PublicationResult
	{
		public readonly	PublicationResultStatus Status;
		public readonly Message Message;
		

		public PublicationResult(
			PublicationResultStatus status,
			Message message)
		{
			this.Status = status;
			this.Message = message;
		}
	}
}
