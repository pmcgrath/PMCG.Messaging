# RabbitMQ .Net library

This library is lightweight library for interacting with RabbitMQ

The following are richer alternatives you should probably consider
* [EasyNetQ](https://github.com/mikehadlow/EasyNetQ)
* [MassTransit](https://github.com/MassTransit/MassTransit)
* [NserviceBus](https://github.com/Particular/NServiceBus)
* [Rebus](https://github.com/rebus-org/Rebus)


## Features
* Connection retries
* Synchronous and asynchronous message publication
* Classifies messages as commands or events based on CQRS messaging ideas
* Events can be published to multiple exchanges
* A publication of a command message will result in an error if no configuration pre-configured
* Transient queues are created for dynamic subscribers and are auto deleted queues, they are named as follows [machinename]_[pid]_[appdomainid]
* All but transient queues are pre-configured


## Build
### Windows
Run build.ps1  
Binaries are placed in the bin sub directory  

### Linux\mono
Run build.sh  
Binaries are placed in the bin sub directory  


## Pre-configuring RabbitMQ
Pending...  


## Sample usage
Will demonstrate connecting to a local instance where user name is guest and password is guest even though is [discouraged since v3.3.0)(http://www.rabbitmq.com/blog/2014/04/02/breaking-things-with-rabbitmq-3-3/) so can replace with your values
### Pre-configure
Enable the management plugin so we can use curl to pre-configure rabbitmq for our sample app
```bash
rabbitmq-plugins enable rabbitmq_management
# Now restart service so we can access the management api on port 15672
```

Pre-configure rabbitmq for the sample app (Will need extra escaping if being run within powershell)
```bash
# Create 2 fanout exchanges
curl -i -u guest:guest -H 'Content-Type:application/json' -d '{ "auto_delete": false, "durable": true, "type": "fanout" }' -X PUT http://localhost:15672/api/exchanges/%2f/test.exchange.1
curl -i -u guest:guest -H 'Content-Type:application/json' -d '{ "auto_delete": false, "durable": true, "type": "fanout" }' -X PUT http://localhost:15672/api/exchanges/%2f/test.exchange.2

# Create a queue
curl -i -u guest:guest -H 'Content-Type:application/json' -d '{ "auto_delete": false, "durable": false, "exclusive": false }' -X PUT http://localhost:15672/api/queues/%2f/test.queue.1

# Bind queue to one of the exchanges
curl -i -u guest:guest -H 'Content-Type:application/json' -d '{ "routing_key": "" }' -X POST http://localhost:15672/api/bindings/%2f/e/test.exchange.1/q/test.queue.1
```
 
### Code in app.cs
```csharp
using PMCG.Messaging;
using PMCG.Messaging.Client;
using PMCG.Messaging.Client.Configuration;
using System;

public class MyEvent : Event
{
    public readonly string Detail; 
    public readonly int Number;

    public MyEvent(
        Guid id,
        string correlationId,
        string detail,
        int number)
        : base(id, correlationId)
    {
        this.Detail = detail;
        this.Number = number;
    }
}

public class MyOtherEvent : Event
{
    public readonly string Detail;
    public readonly int Number;

    public MyOtherEvent(
        Guid id,
        string correlationId,
        string detail,
        int number)
        : base(id, correlationId)
    {
        this.Detail = detail;
        this.Number = number;
    }
}

public class App
{
    public static void Main()
    {
        var _connectionString = "amqp://guest:guest@localhost:5672/";
        var _exchangeName1 = "test.exchange.1";
        var _queueName1 = "test.queue.1";
        var _exchangeName2 = "test.exchange.2";

        Bus _bus = null;  // This allows us to capture the bus in a closure so we can use when publishing from a message handler
        var _configurationBuilder = new BusConfigurationBuilder();
        _configurationBuilder.ConnectionUris.Add(_connectionString);
        _configurationBuilder.RegisterPublication<MyEvent>(_exchangeName1, typeof(MyEvent).Name);
        _configurationBuilder.RegisterPublication<MyOtherEvent>(_exchangeName2, typeof(MyOtherEvent).Name);
        _configurationBuilder.RegisterConsumer<MyEvent>(_queueName1, typeof(MyEvent).Name,
            message =>
                {
                    Console.WriteLine("Consuming message");
                    _bus.PublishAsync(new MyOtherEvent(Guid.NewGuid(), message.CorrelationId, "Pub with closure", message.Number));
                    return ConsumerHandlerResult.Completed;
                });

        _bus = new Bus(_configurationBuilder.Build());
        _bus.Connect();

        Console.WriteLine("Hit enter to publish a message, x to exit");
        Console.ReadLine();
        var _sequence = 1;
        do
        {
            Console.WriteLine("About to publish {0}", _sequence);
            var _message = new MyEvent(Guid.NewGuid(), "Correlation Id", "...", _sequence);

            try
            {
                var _result = _bus.PublishAsync(_message);
                var _completedWithinTimeout = _result.Wait(TimeSpan.FromSeconds(1));
                Console.WriteLine("Completed within timed out [true\false] = {0}, result status is {1}", _completedWithinTimeout, _result.Result.Status);
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
```

### Compile sample as follows  
Place the content of the bin's directory into a local directory  
Place above content in a file named app.cs in the same local directory  
On windows compile using  
```powershell
C:\windows\microsoft.net\framework64\v4.0.30319\csc.exe .\app.cs /r:PMCG.Messaging.Client.dll /r:PMCG.Messaging.dll
```
On linux\mono compile using  
```bash
mcs app.cs /r:PMCG.Messaging.Client.dll /r:PMCG.Messaging.dll
```

