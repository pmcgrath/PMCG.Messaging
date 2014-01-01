Bus states
	Initialised
	Connected
	Blocked
	Disconnected
	Closed



Pending
	Acknowledge threading edge cases
		No locks on state transitions - keep it simple, too simple ?

	Credentials in connection string
		Protect ? DPAPI is too much trouble - different machines

	ConsumerDequeueTimeout
		What is a reasonable value ?

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
