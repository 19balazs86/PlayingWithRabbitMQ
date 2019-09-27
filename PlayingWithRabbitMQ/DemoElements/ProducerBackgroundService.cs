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
      try
      {
        // Publish 1-1 message and dispose/close.
        using (IProducer<PurchaseMessage> purchaseProducer = await _brokerFactory.CreateProducerAsync<PurchaseMessage>(stoppingToken))
        using (IProducer<LoginMessage> loginProducer       = await _brokerFactory.CreateProducerAsync<LoginMessage>(stoppingToken))
        {
          await purchaseProducer.PublishAsync(new PurchaseMessage(), stoppingToken);
          await loginProducer.PublishAsync(new LoginMessage(), stoppingToken);
        }

        // In general, do not need to keep the connection open.
        using (IProducer<PurchaseMessage> purchaseProducer = await _brokerFactory.CreateProducerAsync<PurchaseMessage>(stoppingToken))
        using (IProducer<LoginMessage> loginProducer       = await _brokerFactory.CreateProducerAsync<LoginMessage>(stoppingToken))
        {
          while (!stoppingToken.IsCancellationRequested)
          {
            await purchaseProducer.PublishAsync(new PurchaseMessage(), stoppingToken);
            await loginProducer.PublishAsync(new LoginMessage(), stoppingToken);

            await Task.Delay(_delaySettings.ProducerDelay, stoppingToken);
          }
        }
      }
      catch (OperationCanceledException) { }
      catch (Exception ex)
      {
        Log.Error(ex, "ProducerBackgroundService encountered an exception.");
      }
    }
  }
}
