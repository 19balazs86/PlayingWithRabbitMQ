using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements;
using PlayingWithRabbitMQ.Queue;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using PlayingWithRabbitMQ.Queue.RabbitMQ.Configuration;
using Serilog;

namespace PlayingWithRabbitMQ
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      IHostBuilder hostBuilder = new HostBuilder()
        .UseEnvironment(args.Contains("--prod") ? EnvironmentName.Production : EnvironmentName.Development)
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureServices(configureServices)
        .UseSerilog(configureLogger);

      await hostBuilder.RunConsoleAsync();
    }

    private static void configureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
      IConfiguration configuration = hostContext.Configuration;

      // -- Add: BrokerFactory depending on the environment.
      if (hostContext.HostingEnvironment.IsProduction())
      {
        BrokerFactoryConfiguration brokerFactoryConfiguration = new BrokerFactoryConfiguration
        {
          Url                       = configuration["RabbitMQ_ConnString"],
          DefaultDeadLetterExchange = "message.morgue",
          DefaultDeadLetterQueue    = "message.morgue.sink"
        };

        services.AddSingleton(brokerFactoryConfiguration);

        services.AddSingleton<IBrokerFactory, Queue.RabbitMQ.BrokerFactory>();
      }
      else
        services.AddSingleton<IBrokerFactory, Queue.InMemory.BrokerFactory>();

      // This configuration needs to publish and consume messages.
      services.AddSingleton(configuration);

      // --> Add: Message handlers with Scrutor.
      services.Scan(scan => scan
        .FromEntryAssembly()
          .AddClasses(classes => classes.AssignableTo<IMessageHandler>())
          //.UsingRegistrationStrategy(RegistrationStrategy.Append) // Default is Append.
          .As<IMessageHandler>()
          .WithSingletonLifetime());

      // These handlers will be added by Scrutor.
      //services.AddSingleton<IMessageHandler, LoginMessageHandler>();
      //services.AddSingleton<IMessageHandler, PurchaseMessageHandler>();

      // --> Add: Background services.
      services.AddHostedService<ProducerBackgroundService>(); // Demo purpose.
      services.AddHostedService<ConsumingBackgroundService>();
    }

    private static void configureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configBuilder)
    {
      configBuilder
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables();
    }

    private static void configureLogger(HostBuilderContext hostContext, LoggerConfiguration configuration)
    {
      configuration
        .MinimumLevel.Verbose()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}");
    }
  }
}
