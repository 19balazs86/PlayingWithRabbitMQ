# Playing with RabbitMQ

This .Net Core application is a complete example (framework) to publish and consume messages with [RabbitMQ](https://www.rabbitmq.com) in a convenient way.

[Separate branch](https://github.com/19balazs86/PlayingWithRabbitMQ/tree/netcoreapp2.2) with the .NET Core 2.2 version.

![](https://www.cloudamqp.com/img/docs/camqp.png)

In order to put it into play, you need a RabbitMQ server. Install it locally: [Windows](http://www.rabbitmq.com/install-windows.html) |  Docker | [CloudAMQP](https://www.cloudamqp.com/plans.html) free plan: Little Lemur - For Development.

##### .NET Libraries for RabbitMQ
- **RabbitMQ.Client**: The official client library. [Nuget package](https://www.nuget.org/packages/RabbitMQ.Client) | [GitHub page](https://github.com/rabbitmq/rabbitmq-dotnet-client) | [API Documentation](https://rabbitmq.github.io/rabbitmq-dotnet-client/index.html).
- [Mass Transit](http://masstransit-project.com): CloudAMQP [documentation section](https://www.cloudamqp.com/docs/index.html) has a recommendation for this service bus implementation.
  - [Using MassTransit in .Net](https://mbarkt3sto.hashnode.dev/setting-up-and-using-masstransit-in-aspnet-core) 📓*MBARK*

- [RawRabbit](https://rawrabbit.readthedocs.io) on [GitHub](https://github.com/pardahlman/RawRabbit): Modern .NET client for communication over RabbitMq.
- [Rebus](https://rebus.fm/): .NET service bus - an implementation of several useful messaging patterns.

There is a benefit to start with the RabbitMQ.Client, that you can learn and understand the basics of RabbitMQ.

##### Worth to mention Michael series about the built-in job queues
- [Part 1](https://michaelscodingspot.com/c-job-queues/) - Implementations in Depth.
- [Part 2](https://michaelscodingspot.com/c-job-queues-with-reactive-extensions-and-channels/) - Reactive Extensions and Channels.
- [Part 3](https://michaelscodingspot.com/c-job-queues-part-3-with-tpl-dataflow-and-failure-handling/) - TPL Dataflow and Failure Handling.
- [Performance showdown of job queues](https://michaelscodingspot.com/performance-of-producer-consumer/).
- Leverage [System.Threading.Channels](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels?view=dotnet-plat-ext-3.0) to create an in-memory queue. My repository: [GenericHost](https://github.com/19balazs86/PlayingWithGenericHost).

### Implementations
##### 1)  In-memory (for test)
- By default the application running in development mode and using an in-memory solution.
- This solution is good for test purpose in order to follow the message from the `Producer` to the `Consumer` and handle it.

##### 2)  FileSystem (for test)
- This version is also meant for test purposes.
- The publisher writes the message into a JSON file in the given folder.
- The consumer receives messages using `FileSystemWatcher`.

##### 3)  RabbitMQ
- For production...

##### 4)  Redis
- This is a pub/sub messaging solution, not queuing.

##### 5)  Logger (for test)
- Just write a log...

##### 6) Azure Service Bus

- Using queues and topics for general use without any extra features like duplicate detection and sessions, which is more for special business case.
- Resources: [Playing with Azure Service Bus](https://github.com/19balazs86/AzureServiceBus).

### Components
#### MessageSettingsAttribute
- Just for the RabbitMQ solution, this attribute sits on top of your message class.
- The properties describe the path of the message from exchange to queue.
- This kind of configuration, which needs to create Producer and Consumer.

```csharp
class MessageSettingsAttribute : Attribute
{
    // Message be published into this exchange.
    string ExchangeName

    // Values: Direct, Fanout, Topic.
    ExchangeType ExchangeType

    // If exchangeType is direct or topic, you have to provide the RouteKey.
    string RouteKey

    // Queue name which you want to consume.
    string QueueName

    // Dead letter queue for rejected messages.
    string DeadLetterQueue

    // This tells RabbitMQ not to give more than x message to a worker at a time.
    ushort PrefetchCount
    
    /// Publish the message as Persistent or Transient.
    /// Messages sent as Persistent that are delivered to 'durable' queues will be logged to disk.
    public DeliveryMode DeliveryMode { get; private set; }
}
```

#### BrokerFactory
- With the proper `MessageSettingsAttribute` (RabbitMQ), you can create `Producer` (publish messages) and `Consumer` (receive messages).

```csharp
public interface IBrokerFactory
{
    IProducer<T> CreateProducer();
    IConsumer<T> CreateConsumer();
}
```

#### Producer and Consumer
- When you create a `Producer`, the framework automatically creates the exchange.
- When you create a `Consumer`, the framework automatically creates the queue and make the binding with the exchange.
- No need to create any exchange, queue or binding manually.

#### IMessageHandler< T >
- Your message handler implement this interface.

```csharp
public interface IMessageHandler<T>
{
    Task HandleMessageAsync(T message, CancellationToken cancellationToken);
}
```

#### ConsumerBackgroundService
- This service is responsible to run a `Consumer` in the background in order to receive messages and handle those with the `IMessageHandler<T>`.

```csharp
public class ConsumerBackgroundService<T> : BackgroundService
{
    public ConsumerBackgroundService(IBrokerFactory ..., IMessageHandler<T> ...)

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create Consumer and start consuming messages.
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Waiting for the handler to finish the process.
        // Dispose the consumer to close the connection.
    }

    private async Task handleMessage(IMessage<T> message)
    {
        // Handle the message via calling the proper handler.
        // Acknowledge or reject the message.
        // Handle exceptions.
    }
}
```

#### Example for message handler
- All you need to create custom message handlers for your own business purpose.

```csharp
public class LoginMessageHandler : IMessageHandler<LoginMessage>
{
    public Task HandleMessageAsync(LoginMessage msg, CancellationToken)
    {
        // Your business logic.
        // DB call.
        // HTTP call.
    }
}
```

#### Configure services
- Initialize the DI container.

```csharp
private void configureServices(HostBuilderContext hostContext, IServiceCollection services)
{
    // --> Add: BrokerFactory depending on the environment.
    // But in general the following
    services.AddSingleton(brokerFactoryConfiguration);
    services.AddSingleton<IBrokerFactory, BrokerFactory>();

    // --> Add: Message handlers
    services.AddMessageHandlers();

    // --> Add: Background services.
    services.AddHostedService<ProducerBackgroundService>(); // Demo purpose.

    // Message consumers in BackgroundService.
    services.AddHostedService<ConsumerBackgroundService<LoginMessage>>();
    services.AddHostedService<ConsumerBackgroundService<PurchaseMessage>>();
}
```
#### GenericHost
- Running background processes and using DI container.
- An example in my repository: [Playing with GenericHost](https://github.com/19balazs86/PlayingWithGenericHost).

#### Program.Main
- Run the application.
- The example has `Producers` and `Consumers` working in the background.

```csharp
public static async Task<int> Main(string[] args)
{
    try
    {
        IHostBuilder hostBuilder = new HostBuilder()
            .UseEnvironment(args.Contains("--prod") ? Production : Development)
            .ConfigureAppConfiguration(configureAppConfiguration)
            .ConfigureServices(configureServices)
            .UseSerilog(configureLogger);

        await hostBuilder.RunConsoleAsync();

        return 0;
    }
    catch (Exception ex) { ... }
}
```
