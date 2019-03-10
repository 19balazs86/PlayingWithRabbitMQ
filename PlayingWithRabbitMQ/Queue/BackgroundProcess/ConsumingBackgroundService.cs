using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.Queue.Exceptions;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public class ConsumingBackgroundService : BackgroundService
  {
    private readonly IBrokerFactory _brokerFactory;
    private readonly IEnumerable<IMessageHandler> _messageHandlers;

    private readonly List<IConsumer> _consumers;
    private readonly SynchronizedList<Task> _handlerTasks;

    public ConsumingBackgroundService(IBrokerFactory brokerFactory, IEnumerable<IMessageHandler> messageHandlers,
      IServiceProvider sp)
    {
      _brokerFactory   = brokerFactory;
      _messageHandlers = messageHandlers;

      _consumers    = new List<IConsumer>();
      _handlerTasks = new SynchronizedList<Task>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      Log.Information($"MessageHandler count: {_messageHandlers.Count()}.");

      foreach (IMessageHandler handler in _messageHandlers)
      {
        IConsumer consumer;

        try
        {
          // --> Create: Consumer. 
          consumer = _brokerFactory.CreateConsumer(handler.ConsumerConfiguration, connectionShutdown);
        }
        catch (Exception ex) when (ex is QueueFactoryException || ex is ArgumentException) 
        {
          Log.Error(ex, $"Failed to create Consumer for {handler.GetType().Name}.");

          continue;
        }

        Log.Information($"Start consuming messages - Queue: '{handler.ConsumerConfiguration.QueueName}', Handler: '{handler.GetType().Name}', PrefetchCount: {handler.ConsumerConfiguration.PrefetchCount}.");

        _consumers.Add(consumer);

        // --> Start consuming messages. 
        consumer.MessageSource.ForEachAsync(message => handleMessage(handler, message, stoppingToken), stoppingToken);
      }

      return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      await base.StopAsync(cancellationToken);

      Log.Information("Stop consuming messages.");

      try
      {
        // Waiting for the handlers to finish the process.
        await Task.WhenAll(_handlerTasks);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Unexpected exception occurred during waiting for the handlers to finish the process.");
      }

      // Dispose all consumers to close the connections.
      _consumers.ForEach(consumer => consumer.Dispose());
    }

    private async void handleMessage(IMessageHandler handler, IMessage message, CancellationToken stoppingToken)
    {
      Task handlerTask = handler.HandleMessageAsync(message, stoppingToken);

      _handlerTasks.Add(handlerTask);

      try
      {
        // --> Wait for processing the message.
        await handlerTask;
      }
      catch (MessageException ex)
      {
        Log.Error(ex, $"Failed to Acknowledge or Reject the message with handler({handler.GetType().Name}).");
      }
      catch (Exception ex)
      {
        Log.Error(ex, $"Unhandled exception occurred during processing the message with handler({handler.GetType().Name})." + "Data: {@MessageData}.", message.Data);

        message.Reject();
      }
      finally
      {
        if (!stoppingToken.IsCancellationRequested)
          _handlerTasks.Remove(handlerTask);
      }
    }

    private static void connectionShutdown(string queueName)
      => Log.Error($"Connection is lost with the queue: '{queueName}'.");
  }
}
