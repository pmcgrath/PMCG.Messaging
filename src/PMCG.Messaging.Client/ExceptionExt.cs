using System;


namespace PMCG.Messaging.Client
{
	internal static class ExceptionExt
	{
		public static string InstrumentationString(
			this Exception self)
		{
			Check.RequireArgumentNotNull("self", self);

			// Currently ignoring inner exceptions - may need to re-visit
			return string.Format("{0} {1} - {2}",
				self.GetType().FullName,
				self.Message.Replace(Environment.NewLine, " "),
				self.StackTrace.Replace(Environment.NewLine, "|"));
		}
	}
}
