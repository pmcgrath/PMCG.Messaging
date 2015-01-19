using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Bus
	{
		public void Run_Where_We_Instantiate_And_Try_To_Connect_To_Non_Existent_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri.Replace("5672", "2567/"));	// Wrong port number

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Allow time for connection attempt to fail, check bus state which should be disconnected");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Instantiate_And_Instruct_To_Stop_The_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);

			new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Instruct_To_Stop_The_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Instruct_To_Close_The_Connection_Using_The_DashBoard()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Connect_And_Then_Close()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Transition_Between_States_By_Instructing_The_Broker()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name);

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");

			Console.WriteLine("Block the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat set_vm_memory_high_watermark 0.0000001");

			var _index = 1;
			do
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "", "R1", _index, "09:00", "DDD....");
				var _task = _SUT.PublishAsync(_myEvent);
				_task.Wait();
				_index++;
				Console.WriteLine("Hit enter to publish a message, x to exit");
			} while (Console.ReadLine() != "x");

			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
			_SUT.Close();
		}


		public void Run_Where_We_Publish_A_Null_Message_Results_In_An_Exception()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>(Configuration.ExchangeName2, typeof(MyEvent).Name);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish null message, which should result in an exception");
			Console.ReadLine();
			try
			{
				_SUT.PublishAsync<MyEvent>(null);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>("", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => Configuration.QueueName1);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_To_Two_Exchanges_No_Consumption_For_The_Same_Messsage()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>(Configuration.ExchangeName2, typeof(MyEvent).Name);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), null, "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait();

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					Configuration.QueueName1,
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), null, "R1", 1, "09:00", "DDD....");
			_SUT.PublishAsync(_message);

			Console.WriteLine("Hit enter to display captured message Id");
			Console.ReadLine();
			Console.WriteLine("Captured message Id [{0}]", _capturedMessageId);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_Subject_To_A_Timeout_And_Consume_The_Same_Messsage()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name)
				.RegisterPublication<MyEvent>("A.Different.Exchange", typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					Configuration.QueueName1,
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), null, null, 1, null, "DDD....");

			try
			{
				var _task = _SUT.PublishAsync(_message);
				if (!_task.Wait(TimeSpan.FromMilliseconds(100)))
				{
					Console.WriteLine("Timed out !");
				}
			}
			catch (AggregateException aggregateException)
			{
				Console.WriteLine(aggregateException);
				foreach (var _internalException in aggregateException.InnerExceptions)
				{
					Console.WriteLine(_internalException);
				}
			}
			catch (Exception genericException)
			{
				Console.WriteLine(genericException);
			}

			Console.WriteLine("Captured message Id [{0}]", _capturedMessageId);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name)
				.RegisterConsumer<MyEvent>(
					typeof(MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; },
					Configuration.ExchangeName1);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish message");
			Console.ReadLine();
			var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", 1, "09:00", "....");
			_SUT.PublishAsync(_message);

			Console.WriteLine("Hit enter to display captured message Id");
			Console.ReadLine();
			Console.WriteLine("Captured message Id [{0}]", _capturedMessageId);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_Multiple_Messages_And_Consume_For_The_Same_Messsages()
		{
			var _numberOfMessagesToPublish = 3;
			var _receivedMessages = new ConcurrentStack<MyEvent>();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder
				.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1")
				.RegisterConsumer<MyEvent>(
					Configuration.QueueName1,
					typeof(MyEvent).Name + "v1",
					message => { _receivedMessages.Push(message); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to publish messages {0}", DateTime.Now);
			Console.ReadLine();
			Console.WriteLine("About to publish messages {0}", DateTime.Now);
			for (var _sequence = 1; _sequence <= _numberOfMessagesToPublish; _sequence++)
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:01", "...");
				_SUT.PublishAsync(_message);
				Thread.Sleep(100);
			}
			Console.WriteLine("Completed publishing messages {0}", DateTime.Now);

			Console.WriteLine("Hit enter to verify recieved messages {0}", DateTime.Now);
			var _stopwatch = Stopwatch.StartNew();
			while (_receivedMessages.Count < _numberOfMessagesToPublish)
			{
				Console.WriteLine("{0} Received message count {1}", DateTime.Now, _receivedMessages.Count);
				Thread.Sleep(100);
			}
			_stopwatch.Stop();
			Console.WriteLine("{0} Received message count {1}", _stopwatch.ElapsedMilliseconds, _receivedMessages.Count);

			Console.WriteLine("Hit enter to close");
			Console.ReadLine();
			_SUT.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Consume_From_A_Transient_Queue_On_A_Cluster()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri.Replace("rabbit1", "rabbit2"));
			_busConfigurationBuilder.RegisterConsumer<MyEvent>("RunMessage",
				message =>
					{
						Console.WriteLine("Received message");
						Thread.Sleep(3000);
						return ConsumerHandlerResult.Completed;
					},
				Configuration.ExchangeName2);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to stop consuming");
			Console.ReadLine();
			_SUT.Close();
		}


		public void Run_Where_We_Continuously_Publish_Until_Program_Killed()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1");
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to start publishing messages {0}", DateTime.Now);
			Console.ReadLine();

			var _sequence = 1;
			while(true)
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:01", "...");

				try
				{
					var _result = _SUT.PublishAsync(_message);
					_result.Wait();
					Console.WriteLine("Result status is {0}", _result.Status);
				}
				catch (Exception theException)
				{
					Console.WriteLine("Exception encountered {0}", theException);
				}

				Thread.Sleep(500);
				_sequence++;
			}
		}


		public void Run_Where_We_Publish_1000_Messages_Waiting_On_Result()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1");
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to start publishing messages {0}", DateTime.Now);
			Console.ReadLine();

			var _tasks = new Task[4];
			for (var _taskIndex = 0; _taskIndex < _tasks.Length; _taskIndex++)
			{
				_tasks[_taskIndex] = new Task(() =>
					{
						for (var _sequence = 1; _sequence <= 1000; _sequence++)
						{
							Console.WriteLine("About to publish {0}", _sequence);
							var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:01", "...");

							try
							{
								var _result = _SUT.PublishAsync(_message);
								_result.Wait();
								Console.WriteLine("Result status is {0}", _result.Status);
							}
							catch (Exception theException)
							{
								Console.WriteLine("Exception encountered {0}", theException);
							}
						}
					});
			}

			var _stopwatch = Stopwatch.StartNew();
			foreach(var _task in _tasks) { _task.Start(); }
			Task.WaitAll(_tasks);

			_stopwatch.Stop();
			Console.WriteLine("Done {0} elapsed = {1} ms", DateTime.Now, _stopwatch.ElapsedMilliseconds);
			Console.ReadLine();
			_SUT.Close();
		}


		public void Run_Where_We_Continuously_Publish_Handling_All_Results()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1");
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to start publishing messages {0}", DateTime.Now);
			Console.ReadLine();

			var _sequence = 1;
			while (true)
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:01", "...");

				try
				{
					var _publicationTimeout = TimeSpan.FromTicks(0);
					var _result = _SUT.PublishAsync(_message);
					_result.Wait(_publicationTimeout);

					// PENDING - What do clients do ?
					Console.WriteLine("PENDING");
				}
				catch (Exception theException)
				{
					Console.WriteLine("Exception encountered {0}", theException);
				}

				//Thread.Sleep(5);
				_sequence++;
			}
		}


		public void Run_Where_We_Attempt_A_Publication_Timeout()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1");
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Hit enter to try publishing message");
			Console.ReadLine();

			var _sequence = 1;
			while (true)
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:12", "...");

				try
				{
					var _result = _SUT.PublishAsync(_message);
					var _completedWithinTimeout = _result.Wait(TimeSpan.FromTicks(1));
					Console.WriteLine("Completed within time out = {0}", _completedWithinTimeout);
				}
				catch (Exception theException)
				{
					Console.WriteLine("Exception encountered {0}", theException);
				}

				Thread.Sleep(500);
				_sequence++;
			}
		}


		public void Run_Where_A_Consumer_Also_Publishes_A_Message_Consumer_Handler_Uses_A_Closure_To_Allow_Publishing_Using_The_Same_Bus_Reference()
		{
			PMCG.Messaging.Client.Bus _bus = null;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Configuration.LocalConnectionUri);
			_busConfigurationBuilder.RegisterPublication<MyEvent>(Configuration.ExchangeName1, typeof(MyEvent).Name + "v1");
			_busConfigurationBuilder.RegisterPublication<MyOtherEvent>(Configuration.ExchangeName2, typeof(MyOtherEvent).Name + "v1");
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(Configuration.QueueName1, typeof(MyEvent).Name + "v1",
				message =>
					{
						Console.WriteLine("Consuming message");
						_bus.PublishAsync(new MyOtherEvent(Guid.NewGuid(), message.CorrelationId, "Pub with closure", message.Sequence));
						Thread.Sleep(4000);
						return ConsumerHandlerResult.Completed;
					});

			_bus = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_bus.Connect();

			Console.WriteLine("Hit enter to try publishing message");
			Console.ReadLine();

			var _sequence = 1;
			do
			{
				Console.WriteLine("About to publish {0}", _sequence);
				var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "R1", _sequence, "09:01", "...");

				try
				{
					var _result = _bus.PublishAsync(_message);
					var _completedWithinTimeout = _result.Wait(TimeSpan.FromTicks(1));
					Console.WriteLine("Completed within time out = {0}", _completedWithinTimeout);
				}
				catch (Exception theException)
				{
					Console.WriteLine("Exception encountered {0}", theException);
				}

				_sequence++;
			} while (Console.ReadLine() != "x");

			_bus.Close();
			Console.WriteLine("Hit enter to finish");
			Console.ReadLine();
		}
	}
}
