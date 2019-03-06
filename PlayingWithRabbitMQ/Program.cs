using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements;
using PlayingWithRabbitMQ.RabbitMQ;
using PlayingWithRabbitMQ.RabbitMQ.BackgroundProcess;
using PlayingWithRabbitMQ.RabbitMQ.Configuration;
using Serilog;

namespace PlayingWithRabbitMQ
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      IHostBuilder hostBuilder = new HostBuilder()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureServices(configureServices)
        .UseSerilog(configureLogger);

      await hostBuilder.RunConsoleAsync();
    }

    private static void configureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
      IConfiguration configuration = hostContext.Configuration;

      BrokerFactoryConfiguration brokerFactoryConfiguration = new BrokerFactoryConfiguration
      {
        Url                       = configuration["RabbitMQ_ConnString"],
        DefaultDeadLetterExchange = "message.morgue",
        DefaultDeadLetterQueue    = "message.morgue.sink"
      };

      services.AddSingleton(brokerFactoryConfiguration);
      services.AddSingleton<IBrokerFactory, BrokerFactory>();

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
