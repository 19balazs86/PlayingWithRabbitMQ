using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue;
using Serilog;

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
      using (IProducer<PurchaseMessage> purchaseProducer = await _brokerFactory.CreateProducerAsync<PurchaseMessage>())
      using (IProducer<LoginMessage> loginProducer       = await _brokerFactory.CreateProducerAsync<LoginMessage>())
      {
        await purchaseProducer.PublishAsync(new PurchaseMessage(), stoppingToken);
        await loginProducer.PublishAsync(new LoginMessage(), stoppingToken);
      }

      // In general, do not need to keep the connection open.
      using (IProducer<PurchaseMessage> purchaseProducer = await _brokerFactory.CreateProducerAsync<PurchaseMessage>())
      using (IProducer<LoginMessage> loginProducer       = await _brokerFactory.CreateProducerAsync<LoginMessage>())
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            await purchaseProducer.PublishAsync(new PurchaseMessage(), stoppingToken);
            await loginProducer.PublishAsync(new LoginMessage(), stoppingToken);
          }
          catch (Exception ex)
          {
            Log.Error(ex, "Failed to publish a message.");

            await Task.Delay(5000, stoppingToken);
          }

          await Task.Delay(_delaySettings.ProducerDelay, stoppingToken);
        }
      }
    }
  }
}
