using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue;
using PlayingWithRabbitMQ.Queue.Configuration;

namespace PlayingWithRabbitMQ.DemoElements
{
  /// <summary>
  /// This class is just for demo purpose to publish messages.
  /// </summary>
  public class ProducerBackgroundService : BackgroundService
  {
    private readonly IBrokerFactory _brokerFactory;
    private readonly IConfiguration _configuration;

    public ProducerBackgroundService(IBrokerFactory brokerFactory, IConfiguration configuration)
    {
      _brokerFactory = brokerFactory;
      _configuration = configuration;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      ProducerConfiguration orderServiceConfig = _configuration.BindTo<ProducerConfiguration>("Producer:OrderService");
      ProducerConfiguration userServiceConfig  = _configuration.BindTo<ProducerConfiguration>("Producer:UserService");

      // In general, you do not need to keep the connection open.
      using (IProducer orderProducer = _brokerFactory.CreateProducer(orderServiceConfig))
      using (IProducer userProducer  = _brokerFactory.CreateProducer(userServiceConfig))
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          orderProducer.Publish(new PurchaseMessage());
          userProducer.Publish(new LoginMessage());

          await Task.Delay(1000, stoppingToken);
        }
      }
    }
  }
}
