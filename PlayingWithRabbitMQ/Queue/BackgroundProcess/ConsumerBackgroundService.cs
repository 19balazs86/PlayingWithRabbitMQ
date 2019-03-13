using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.Queue.Exceptions;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public class ConsumerBackgroundService<T> : BackgroundService where T : class, new()
  {
    private readonly IBrokerFactory _brokerFactory;
    private readonly IMessageHandler<T> _messageHandler;

    private IConsumer<T> _consumer;
    private int _handlerCounter = 0;

    public ConsumerBackgroundService(IBrokerFactory brokerFactory, IMessageHandler<T> messageHandler)
    {
      _brokerFactory  = brokerFactory;
      _messageHandler = messageHandler;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        // --> Create: Consumer. 
        _consumer = _brokerFactory.CreateConsumer<T>(connectionShutdown);

        Log.Information($"Start consuming messages(type: {typeof(T).Name}).");

        // --> Start consuming messages. 
        _consumer.MessageSource.Subscribe(message => handleMessage(message, stoppingToken), stoppingToken);
      }
      catch (Exception ex)
      {
        Log.Error(ex, $"Failed to create Consumer for {typeof(T).Name}.");
      }

      return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      await base.StopAsync(cancellationToken);

      Log.Information($"Stop consuming {typeof(T).Name}.");

      // Waiting for the handlers to finish the process.
      while (Interlocked.CompareExchange(ref _handlerCounter, 0, 0) > 0)
        await Task.Delay(100);

      // Dispose consumer to close the connections.
      _consumer?.Dispose();
    }

    private async void handleMessage(IMessage<T> message, CancellationToken stoppingToken)
    {
      Interlocked.Increment(ref _handlerCounter);

      bool? isRequeue = null;

      try
      {
        await _messageHandler.HandleMessageAsync(message.Item, stoppingToken);

        message.Acknowledge();

        Log.Verbose($"Acknowledge: {typeof(T).Name}.");
      }
      catch (OperationCanceledException ex)
      {
        Log.Warning(ex, $"The operation was canceled. The {typeof(T).Name} will be requeue.");

        isRequeue = true;
      }
      catch (MessageException ex)
      {
        Log.Error(ex, $"Failed to acknowledge the {typeof(T).Name}.");

        isRequeue = false;
      }
      catch (Exception ex)
      {
        Log.Error(ex, $"An exception occurred during processing the {typeof(T).Name}.");

        isRequeue = false;
      }

      // If an exception occurred.
      if (isRequeue.HasValue)
      {
        try
        {
          message.Reject(requeue: isRequeue.Value);

          Log.Debug("Message rejected: {@RawItem}.", message.RawItem);
        }
        catch (Exception ex)
        {
          Log.Error(ex, $"Failed to reject the {typeof(T).Name}.");
        }
      }

      Interlocked.Decrement(ref _handlerCounter);
    }

    private static void connectionShutdown()
      => Log.Error($"Connection is lost with the {typeof(T).Name} Consumer.");
  }
}
