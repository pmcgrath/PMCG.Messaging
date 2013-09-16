using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.DisconnectedStorage
{
	public interface IStore
	{
		IEnumerable<Guid> GetAllIds();

		void Add(
			Message message);

		Message Get(
			Guid id);

		void Delete(
			Guid id);
	}
}