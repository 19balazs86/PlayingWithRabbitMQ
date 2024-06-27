using PlayingWithRabbitMQ.DemoElements;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using PlayingWithRabbitMQ.Queue.RabbitMQ;
using Serilog;
using Serilog.Events;

namespace PlayingWithRabbitMQ;

public static class Program
{
    public static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSerilog(configureSerilog);

        var services      = builder.Services;
        var configuration = builder.Configuration;

        // Add services to the container
        {
            // --> Add: BrokerFactory depending on the environment.
            if (builder.Environment.IsProduction())
            {
                var brokerFactoryConfiguration = new BrokerFactoryConfiguration
                {
                    Url                       = configuration.GetConnectionString("RabbitMQ"),
                    SkipManagement            = false,
                    DefaultDeadLetterExchange = "message.morgue",
                    DefaultDeadLetterQueue    = "message.morgue.sink"
                };

                services.AddSingleton(brokerFactoryConfiguration);

                services.AddSingleton<IBrokerFactory, BrokerFactory>();
            }
            else
            {
                // --> In-memory queuing
                services.AddSingleton<IBrokerFactory, Queue.InMemory.BrokerFactory>();

                // --> File system queuing
                //services.AddSingleton<IBrokerFactory>(new Queue.FileSystem.BrokerFactory(@"d:\Downloads\Messages"));

                // --> Redis pub/sub messaging
                //services.AddSingleton<IBrokerFactory>(new Queue.Redis.BrokerFactory());

                var sbConfiguration = new Queue.Azure.ServiceBus.ServiceBusConfiguration
                {
                    ConnectionString = configuration.GetConnectionString("ServiceBus"),
                    SkipManagement = false
                };

                // --> Azure: Queue
                //services.AddSingleton<IBrokerFactory>(x => new Queue.Azure.ServiceBus.Queue.BrokerFactory(sbConfiguration));

                // --> Azure: Topic
                //services.AddSingleton<IBrokerFactory>(x => new Queue.Azure.ServiceBus.Topic.BrokerFactory(sbConfiguration));
            }

            // --> Add: DelaySettings
            services.AddSingleton(configuration.GetSection(nameof(DelaySettings)).Get<DelaySettings>());

            // --> Add: Message handlers with own extension method
            services.AddMessageHandlers(ServiceLifetime.Singleton);

            // --> Add: Background services
            // Message consumers in BackgroundService
            services.AddHostedService<ConsumerBackgroundService<LoginMessage>>();
            services.AddHostedService<ConsumerBackgroundService<PurchaseMessage>>();

            // Demo purpose.
            services.AddHostedService<ProducerBackgroundService>();
        }

        builder.Build().Run();
    }
    private static void configureSerilog(LoggerConfiguration configuration)
    {
        configuration
            .MinimumLevel.Debug()
            .MinimumLevel.Override("PlayingWithRabbitMQ.Queue.BackgroundProcess", LogEventLevel.Information)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}");
    }
}
