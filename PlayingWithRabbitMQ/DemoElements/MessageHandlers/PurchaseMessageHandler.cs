using System;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using Serilog;

namespace PlayingWithRabbitMQ.DemoElements.MessageHandlers
{
  public class PurchaseMessageHandler : IMessageHandler<PurchaseMessage>
  {
    private static readonly Random _random = new Random();
    private readonly DelaySettings _delaySettings;

    public PurchaseMessageHandler(DelaySettings delaySettings)
    {
      _delaySettings = delaySettings;
    }

    public async Task HandleMessageAsync(PurchaseMessage message, CancellationToken cancellationToken = default)
    {
      Log.Debug($"{GetType().Name}: Message is processing. Id: '{message.Id}'.");

      // This method will be finished, even if the service is stopped.

      // Task #1: Without cancellationToken.
      await Task.Delay(_delaySettings.HandlerDelay);

      // Task #2: Without cancellationToken.
      await Task.Delay(_delaySettings.HandlerDelay);

      if (_random.NextDouble() <= 0.10)
        throw new Exception($"Just a random Exception from { GetType().Name }");
    }
  }
}
