using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue;

namespace PlayingWithRabbitMQ.DemoElements
{
  /// <summary>
  /// This class is just for demo purposes to publish messages.
  /// </summary>
  public class ProducerBackgroundService : BackgroundService
  {
    private readonly IBrokerFactory _brokerFactory;
    private readonly DelaySettings _delaySettings;

    public ProducerBackgroundService(IBrokerFactory brokerFactory, DelaySettings delaySettings)
    {
      _brokerFactory = brokerFactory;
      _delaySettings = delaySettings;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // Publish 1-1 message and dispose/close.
      using (IProducer<PurchaseMessage> purchaseProducer = _brokerFactory.CreateProducer<PurchaseMessage>())
      using (IProducer<LoginMessage> loginProducer       = _brokerFactory.CreateProducer<LoginMessage>())
      {
        await purchaseProducer.PublishAsync(new PurchaseMessage());
        await loginProducer.PublishAsync(new LoginMessage());
      }

      // In general, do not need to keep the connection open.
      using (IProducer<PurchaseMessage> purchaseProducer = _brokerFactory.CreateProducer<PurchaseMessage>())
      using (IProducer<LoginMessage> loginProducer       = _brokerFactory.CreateProducer<LoginMessage>())
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          await purchaseProducer.PublishAsync(new PurchaseMessage());
          await loginProducer.PublishAsync(new LoginMessage());

          await Task.Delay(_delaySettings.ProducerDelay, stoppingToken);
        }
      }
    }
  }
}
