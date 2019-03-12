using System;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using Serilog;

namespace PlayingWithRabbitMQ.DemoElements.MessageHandlers
{
  public class LoginMessageHandler : IMessageHandler<LoginMessage>
  {
    private static readonly Random _random = new Random();
    private readonly DelaySettings _delaySettings;

    public LoginMessageHandler(DelaySettings delaySettings)
    {
      _delaySettings = delaySettings;
    }

    public async Task HandleMessageAsync(LoginMessage message, CancellationToken cancellationToken = default)
    {
      Log.Debug($"{GetType().Name}: Message is processing. UserId: '{message.UserId}'.");

      // Task #1: Without cancellationToken.
      await Task.Delay(_delaySettings.HandlerDelay);

      // According to the businnes logic, the method throws exception, if the service is stopped before here.
      // The message will be requeue.
      cancellationToken.ThrowIfCancellationRequested();

      // After this point the method will be finished, even if the service is stopped.

      // Task #2: Without cancellationToken.
      await Task.Delay(_delaySettings.HandlerDelay);

      if (_random.NextDouble() <= 0.10)
        throw new Exception($"Just a random Exception from { GetType().Name }");
    }
  }
}
