using System;


namespace PMCG.Messaging
{
	public interface IBusController
	{
		BusStatus Status { get; }

		void Connect();

		void Close();
	}
}