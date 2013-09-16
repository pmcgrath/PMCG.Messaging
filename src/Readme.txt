Pending
	Logging
		Abstraction so we can plug log4net and Nlog in
		Start using LogManager concept - is NLog the same as log4net
		Nlog supports syslog ?

	Publication
		Lost messages due to being added to blocking queue but not already sent - process dies, messages lost ?
			Does the publish need to be synchronous

	Acknowledge threading edge cases
		No locks on state transitions - keep it simple, too simple ?

	Clustering
		Support optional multiple hosts

	Credentials in connection string
		Protect ? DPAPI is too much trouble - different machines

	SubscriptionDequeueTimeout
		What is a reasonable value ?

	Dicsonnected strategies
		Redis\Esent alternatives

	Write explanations of
		Initial connection
			Blocking
		Configuration
			Static and transient queue's
		BackAcks event handler
			Publisher confirms
		Transient queues
			Naming strategy, exclusive flag setting
		DLX
			Usage
		Disconnect storage strategy
			On startup
			On disconnected
			TTL skew - file is 3 days old
