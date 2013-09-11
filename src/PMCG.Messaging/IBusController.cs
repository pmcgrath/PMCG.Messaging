using System;


namespace PMCG.Messaging
{
	public interface IBusController
	{
		void Connect();

		void Close();
	}
}