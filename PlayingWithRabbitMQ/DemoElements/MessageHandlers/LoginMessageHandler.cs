using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlayingWithRabbitMQ.DemoElements.Messages;
using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using PlayingWithRabbitMQ.Queue.Configuration;
using Serilog;

namespace PlayingWithRabbitMQ.DemoElements.MessageHandlers
{
  public class LoginMessageHandler : MessageHandlerBase<LoginMessage>
  {
    private static readonly Random _random = new Random();

    public LoginMessageHandler(IConfiguration configuration) :
      base(configuration.BindTo<ConsumerConfiguration>("Consumer:StatisticsService"))
    {

    }

    public override async Task HandleMessageAsync(LoginMessage message, CancellationToken cancellationToken = default)
    {
      Log.Debug($"{GetType().Name}: Message is processing. UserId: '{message.UserId}'.");

      // Task #1: Without cancellationToken.
      await Task.Delay(_random.Next(500, 1000));

      // According to the businnes logic, the method throws exception, if the service is stopped before here.
      // The message will be requeue.
      cancellationToken.ThrowIfCancellationRequested();

      // After this point the method will be finished, even if the service is stopped.

      // Task #2: Without cancellationToken.
      await Task.Delay(_random.Next(500, 1000));

      if (_random.NextDouble() <= 0.10)
        throw new Exception($"Just a random Exception from { GetType().Name }");
    }
  }
}
