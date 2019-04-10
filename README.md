# Playing with RabbitMQ

This .Net Core application is a complete example (framework) to publish and consume messages with [RabbitMQ](https://www.rabbitmq.com "RabbitMQ") in a convenient way.

![](https://www.cloudamqp.com/img/docs/camqp.png)

In order to put it into play, you need a RabbitMQ server. Install it locally: [Windows](http://www.rabbitmq.com/install-windows.html "Windows") |  Docker | [CloudAMQP](https://www.cloudamqp.com/plans.html "CloudAMQP") free plan: Little Lemur - For Development.

##### .NET Libraries for RabbitMQ
- RabbitMQ.Client: The official client library. [Nuget package](https://www.nuget.org/packages/RabbitMQ.Client "Nuget package") | [GitHub page](https://github.com/rabbitmq/rabbitmq-dotnet-client "GitHub page") | [API Documentation](https://rabbitmq.github.io/rabbitmq-dotnet-client/index.html "API Documentation").
- [Mass Transit](http://masstransit-project.com "Mass Transit"): CloudAMQP [documentation section](https://www.cloudamqp.com/docs/index.html "documentation section") has a recommendation for this service bus implementation.
- [RawRabbit](https://rawrabbit.readthedocs.io/en/master "RawRabbit"): Modern .NET client for communication over RabbitMq, which is written for .NET Core. [GitHub page](https://github.com/pardahlman/RawRabbit "GitHub page").

There is a benefit to start with the RabbitMQ.Client, that you can learn and understand the basics of RabbitMQ.

##### Worth to mention Michael series about the built-in job queues
- [Part 1](https://michaelscodingspot.com/c-job-queues/ "Part 1") - Implementations in Depth.
- [Part 2](https://michaelscodingspot.com/c-job-queues-with-reactive-extensions-and-channels/ "Part 2") - Reactive Extensions and Channels.
- [Part 3](https://michaelscodingspot.com/c-job-queues-part-3-with-tpl-dataflow-and-failure-handling/ "Part 3") - TPL Dataflow and Failure Handling.

### Components
#### In-memory solution for test
- By default the application running in development mode and using an in-memory solution.
- This solution is good for test purpose in order to follow the message from the Producer to the Consumer and handle it.

#### QueueMessageAttribute
- This attribute sits on top of your message class.
- The properties describe the path of the message from exchange to queue.
- This kind of configuration, which needs to create Producer and Consumer.

```csharp
class QueueMessageAttribute : Attribute
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
}
```

#### BrokerFactory
- With the proper configuration of QueueMessageAttribute, you can create **Producer** (publish messages) and **Consumer** (receive messages).

```csharp
public interface IBrokerFactory
{
    IProducer<T> CreateProducer();
    IConsumer<T> CreateConsumer();
}
```

#### Producer and Consumer
- When you create a Producer, the framework automatically creates the exchange.
- When you create a Consumer, the framework automatically creates the queue and make the binding with the exchange.
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
- This service is responsible to run a Consumer in the background in order to receive messages and handle those with the **IMessageHandler< T >**.

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
- [Scrutor](https://github.com/khellang/Scrutor "Scrutor"): [Using Scrutor to automatically register services with ASP.NET Core DI container](https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container "Using Scrutor to automatically register services with ASP.NET Core DI container").

```csharp
private void configureServices(HostBuilderContext hostContext, IServiceCollection services)
{
    // --> Add: BrokerFactory depending on the environment.
    // But in general the following
    services.AddSingleton(brokerFactoryConfiguration);
    services.AddSingleton<IBrokerFactory, BrokerFactory>();

    // --> Add: Message handlers with Scrutor.
    services.Scan(scan => scan
        .FromEntryAssembly()
            .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))ó
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

    // --> Add: Background services.
    services.AddHostedService<ProducerBackgroundService>(); // Demo purpose.

    // Message consumers in BackgroundService.
    services.AddHostedService<ConsumerBackgroundService<LoginMessage>>();
    services.AddHostedService<ConsumerBackgroundService<PurchaseMessage>>();
}
```
#### GenericHost
- Running background processes and using DI container.
- An example in my repository: [Playing with GenericHost](https://github.com/19balazs86/PlayingWithGenericHost "Playing with GenericHost").

#### Program.Main
- Run the application.
- The example has Producers and Consumers working in the background.

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