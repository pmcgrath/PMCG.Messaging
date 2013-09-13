Pending
	Logging
		Abstraction so we can plug log4net and Nlog in
		Start using LogManager concept - is NLog the same as log4net
		Nlog supports syslog ?

	Publication
		Lost messages due to being added to blocking quue but not already sent - process dies, messages lost ?
			Does the publish need to be synchronous

	Acknowledge threading edge cases
		No locks on state transitions - keep it simple, too simple ?

	Clustering
		Support optional multiple hosts

	Credentials in connection string
		Protect ? DPAPI is too much trouble - different machines

	SubscriptionDequeueTimeout
		What is a reasonable value ?


