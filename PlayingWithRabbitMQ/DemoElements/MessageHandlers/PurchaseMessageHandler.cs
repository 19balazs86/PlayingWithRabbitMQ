using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.RabbitMQ.BackgroundProcess;
using PlayingWithRabbitMQ.RabbitMQ.Configuration;
using Serilog;

namespace PlayingWithRabbitMQ.DemoElements.MessageHandlers
{
  public class PurchaseMessageHandler : MessageHandlerBase<PurchaseMessage>
  {
    private static readonly Random _random = new Random();

    public PurchaseMessageHandler(IConfiguration configuration) :
      base(configuration.BindTo<ConsumerConfiguration>("Consumer:ShippingService"))
    {

    }

    public override async Task HandleMessageAsync(PurchaseMessage message, CancellationToken cancellationToken = default)
    {
      Log.Debug($"{GetType().Name}: Message is processing. Id: '{message.Id}'.");

      // This method will be finished, even if the service is stopped.

      // Task #1: Without cancellationToken.
      await Task.Delay(_random.Next(500, 1000));

      // Task #2: Without cancellationToken.
      await Task.Delay(_random.Next(500, 1000));

      if (_random.NextDouble() <= 0.10)
        throw new Exception($"Just a random Exception from { GetType().Name }");
    }
  }
}
