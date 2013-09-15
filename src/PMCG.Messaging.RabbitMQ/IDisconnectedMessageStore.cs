﻿using System;
using System.Collections.Generic;


namespace PMCG.Messaging.RabbitMQ
{
	public interface IDisconnectedMessageStore
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