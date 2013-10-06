using System;


namespace PMCG.Messaging.Client
{
	public static class Check
	{
		public static void RequireArgument(
			string argumentName,
			object argumentValue,
			bool isConditionSatisfied)
		{
			if (!isConditionSatisfied) { throw new ArgumentException(string.Format("Invalid value {0}", argumentValue), argumentName); }
		}

	
		public static void RequireArgumentNotEmpty(
			string argumentName,
			string argumentValue)
		{
			if (string.IsNullOrWhiteSpace(argumentValue)) { throw new ArgumentException("Cannot be empty", argumentName); }
		}


		public static void RequireArgumentNotNull(
			string argumentName,
			object argumentValue)
		{
			if (argumentValue == null) { throw new ArgumentNullException(argumentName); }
		}


		public static void Ensure(
			bool isConditionSatisfied,
			string conditionNotSatisfiedMessage)
		{
			if (!isConditionSatisfied) { throw new ApplicationException(string.Format("Pre-condition not satisfied - {0}", conditionNotSatisfiedMessage)); }
		}
	}
}