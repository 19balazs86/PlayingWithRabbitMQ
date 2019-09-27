﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using PlayingWithRabbitMQ.Queue.RabbitMQ;
using Serilog;
using Serilog.Events;

namespace PlayingWithRabbitMQ
{
  public class Program
  {
    public static async Task<int> Main(string[] args)
    {
      try
      {
        IHostBuilder hostBuilder = new HostBuilder()
          .UseEnvironment(args.Contains("--prod") ? EnvironmentName.Production : EnvironmentName.Development)
          .ConfigureAppConfiguration(configureAppConfiguration)
          .ConfigureServices(configureServices)
          .UseSerilog(configureLogger);

        await hostBuilder.RunConsoleAsync();

        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An exception occurred starting the Host. Message: '{ex.Message}'");

        return -1;
      }
    }

    private static void configureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
      IConfiguration configuration = hostContext.Configuration;

      // --> Add: BrokerFactory depending on the environment.
      if (hostContext.HostingEnvironment.IsProduction())
      {
        var brokerFactoryConfiguration = new BrokerFactoryConfiguration
        {
          Url = configuration.GetConnectionString("RabbitMQ"),
          DefaultDeadLetterExchange = "message.morgue",
          DefaultDeadLetterQueue    = "message.morgue.sink"
        };

        services
          .AddSingleton(brokerFactoryConfiguration)
          .AddSingleton<IBrokerFactory, BrokerFactory>();
      }
      else
      {
        // In-memory queuing.
        services.AddSingleton<IBrokerFactory, Queue.InMemory.BrokerFactory>();

        // File system queuing.
        //services.AddSingleton<IBrokerFactory>(new Queue.FileSystem.BrokerFactory(@"d:\Downloads\Messages"));

        // Redis pub/sub messaging.
        //services.AddSingleton<IBrokerFactory>(new Queue.Redis.BrokerFactory());

        // Azure queue.
        //services.AddSingleton<IBrokerFactory>(x => new Queue.Azure.ServiceBus.Queue.BrokerFactory(configuration.GetConnectionString("ServiceBus")));
      }

      // --> Add: DelaySettings.
      services.AddSingleton(configuration.BindTo<DelaySettings>());

      // --> Add: Message handlers with Scrutor.
      services.Scan(scan => scan
        .FromEntryAssembly()
          .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
          //.UsingRegistrationStrategy(RegistrationStrategy.Append) // Default is Append.
          .AsImplementedInterfaces()
          .WithSingletonLifetime());
      // Note: If the handler has scope dependencies, it should be present as a scope handler.

      // These handlers will be added by Scrutor.
      //services.AddSingleton<IMessageHandler<LoginMessage>, LoginMessageHandler>();
      //services.AddSingleton<IMessageHandler<PurchaseMessage>, PurchaseMessageHandler>();

      // --> Add: Background services.
      // Message consumers in BackgroundService.
      services.AddHostedService<ConsumerBackgroundService<LoginMessage>>();
      services.AddHostedService<ConsumerBackgroundService<PurchaseMessage>>();

      // Demo purpose.
      services.AddHostedService<ProducerBackgroundService>();
    }

    private static void configureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configBuilder)
    {
      configBuilder
        .AddJsonFile("appsettings.json", false)
        .AddEnvironmentVariables();

      // Custom connection string from environment variable, key: CUSTOMCONNSTR_RabbitMQ.
    }

    private static void configureLogger(HostBuilderContext hostContext, LoggerConfiguration configuration)
    {
      configuration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("PlayingWithRabbitMQ.Queue.BackgroundProcess", LogEventLevel.Information)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}");
    }
  }
}
