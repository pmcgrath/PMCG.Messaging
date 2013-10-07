﻿using System;


namespace PMCG.Messaging
{
	public abstract class Command : Message
	{
		protected Command(
			Guid id,
			string correlationId)
			: base(id, correlationId)
		{
		}
	}
}