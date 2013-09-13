using PMCG.Messaging.RabbitMQ.Configuration;
using System;


namespace PMCGMessaging.RabbitMQ.Interactive
{
	public static class ConfigurationFactory
	{
		public static BusConfiguration CreateABusConfiguration()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUri = "......";
			_busConfigurationBuilder.DisconnectedMessagesStoragePath = @"D:\temp\rabbitdisconnectedmessages";
			_busConfigurationBuilder.RegisterSubscription<TheEvent>(
				"pcs.offerevents.fxs",
				typeof(TheEvent).Name,
				message =>
					{
						return MessageSubscriptionActionResult.Completed;
					});
			_busConfigurationBuilder.RegisterSubscription<TheEvent>(
				"pcs.offerevents.fxs",
				typeof(TheEvent).FullName,
				message =>
					{
						return MessageSubscriptionActionResult.Completed;
					});
			_busConfigurationBuilder.RegisterSubscription<TheEvent>(
				"pcs.offerevents.fxs",
				"Throw_Error_Type_Header",
				message =>
					{
						throw new ApplicationException("Bang !");
					});
			_busConfigurationBuilder.RegisterSubscription<TheEvent>(
				"pcs.offerevents.fxs",
				"Returns_Errored_Result",
				message =>
					{
						return MessageSubscriptionActionResult.Errored;
					});

			return _busConfigurationBuilder.Build();
		}
	}
}
