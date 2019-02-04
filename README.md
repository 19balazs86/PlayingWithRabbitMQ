# Playing with RabbitMQ

This .Net Core application is a complete example (framework) to publish and consume messages with [RabbitMQ](https://www.rabbitmq.com "RabbitMQ") in a convenient way.

![](https://www.cloudamqp.com/img/docs/camqp.png)

In order to put it into play, you need a RabbitMQ server.

- Server: Locally: [Windows](http://www.rabbitmq.com/install-windows.html "Windows") |  Docker | [CloudAMQP](https://www.cloudamqp.com/plans.html "CloudAMQP") free plan: Little Lemur - For Development.
- [Nuget package](https://www.nuget.org/packages/RabbitMQ.Client "Nuget package") for RabbitMQ .NET Client.
- [API Documentation](https://rabbitmq.github.io/rabbitmq-dotnet-client/index.html "API Documentation").
- [GitHub page](https://github.com/rabbitmq/rabbitmq-dotnet-client "GitHub page").

CloudAMQP [documentation section](https://www.cloudamqp.com/docs/index.html "documentation section") has a recommendation for [Mass Transit](http://masstransit-project.com "Mass Transit") - "A Service Bus Implementation for .NET with RabbitMQ support." This is such a powerful framework, layer over RabbitMQ. There is a benefit to start with vanilia .NET Client, that you can learn and understand the basics of RabbitMQ.

### Components
#### BrokerFactory
- With the proper configuration you can create **Producer** (publish messages) and **Consumer** (consume messages).

```csharp
public interface IBrokerFactory
{
    IProducer CreateProducer(ProducerConfiguration configuration);
    IConsumer CreateConsumer(ConsumerConfiguration configuration);
}
```

#### Producer and Consumer
- When you create a Producer, the framework automatically creates the exchange according to the given configuration.
- When you create a Consumer, the framework automatically creates the queue and make the binding with the exchange according to the configuration.
- No need to create any exchange, queue or binding manually.

#### ConsumingBackgroundService
- This service is responsible to run Consumers in the background in order to consume messages and handle those with the **IMessageHandler**.

```csharp
public class ConsumingBackgroundService : BackgroundService
{
    public ConsumingBackgroundService(IBrokerFactory ..., IEnumerable<IMessageHandler> ...)

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Creates Consumers according to the given configuration from the IMessageHandler.
        // Start consuming messages.
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Waiting for the handlers to finish the process.
        // Dispose all consumers to close the connections.
    }

    private async void handleMessage(IMessageHandler handler, IMessage msg, CancellationToken)
    {
        // Handle the message via calling the handler.HandleMessageAsync(msg).
    }
}
```

#### IMessageHandler

```csharp
public interface IMessageHandler
{
    ConsumerConfiguration ConsumerConfiguration { get; }
    Task HandleMessageAsync(IMessage message, CancellationToken ct);
}
```

#### MessageHandlerBase
- Abstract implementation for IMessageHandler.

```csharp
public abstract class MessageHandlerBase<TMessage> : IMessageHandler
{
    // ...
    public abstract Task HandleMessageAsync(TMessage message, CancellationToken ct);

    public async Task HandleMessageAsync(IMessage message, CancellationToken ct)
    {
        // 1. Deserialize the message.
        // 2. Call the typed HandleMessageAsync method.
        // 3. Acknowledge or reject the message.
        // 4. Handle exceptions.
    }
}
```

#### Consuming queue and handle messages
- All you need to create custom message handlers for your own business purpose using the MessageHandlerBase as a base class.

```csharp
public class LoginMessageHandler : MessageHandlerBase<LoginMessage>
{
    public LoginMessageHandler(IConfiguration configuration) :
        base(configuration.BindTo<ConsumerConfiguration>("ConsumerX")) { }

    public override async Task HandleMessageAsync(LoginMessage msg, CancellationToken)
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
    // ...
    services.AddSingleton(brokerFactoryConfiguration);
    services.AddSingleton<IBrokerFactory, BrokerFactory>();

    services.AddSingleton(configuration);

    // --> Add: Message handlers.
    services.AddSingleton<IMessageHandler, LoginMessageHandler>();
    services.AddSingleton<IMessageHandler, PurchaseMessageHandler>();

    // --> Add: Background services.
    services.AddHostedService<ProducerBackgroundService>(); // Demo purpose.
    services.AddHostedService<ConsumingBackgroundService>();
}
```
#### GenericHost
- Running background processes and using DI container.
- An example in my repository: [Playing with GenericHost](https://github.com/19balazs86/PlayingWithGenericHost "Playing with GenericHost").

#### Program.Main
- Run the application.
- The example has Producers and Consumers working in the background.

```csharp
public static async Task Main(string[] args)
{
    IHostBuilder hostBuilder = new HostBuilder()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureServices(configureServices)
        .UseSerilog(configureLogger);

    await hostBuilder.RunConsoleAsync();
}
```