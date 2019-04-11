using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Hosting;
using PlayingWithRabbitMQ.Queue.Exceptions;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public class ConsumerBackgroundService<T> : BackgroundService where T : class, new()
  {
    private readonly IBrokerFactory _brokerFactory;
    private readonly IMessageHandler<T> _messageHandler;

    private readonly ActionBlock<IMessage<T>> _actionBlock;

    private IConsumer<T> _consumer;

    private CancellationToken _stoppingToken;

    public ConsumerBackgroundService(IBrokerFactory brokerFactory, IMessageHandler<T> messageHandler)
    {
      _brokerFactory  = brokerFactory;
      _messageHandler = messageHandler;

      // Set the MaxDegreeOfParallelism value.
      var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

      _actionBlock = new ActionBlock<IMessage<T>>(handleMessage, options);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _stoppingToken = stoppingToken;

      try
      {
        // --> Create: Consumer.
        _consumer = _brokerFactory.CreateConsumer<T>();

        Log.Information($"Start consuming messages(type: {typeof(T).Name}).");

        // --> Start consuming messages. 
        _consumer.MessageSource.Subscribe(message => _actionBlock.Post(message), stoppingToken);
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

      _actionBlock.Complete();

      // Waiting for the handlers to finish the process.
      await _actionBlock.Completion;

      // Dispose consumer to close the connection.
      _consumer?.Dispose();
    }

    private async Task handleMessage(IMessage<T> message)
    {
      if (_stoppingToken.IsCancellationRequested) return;

      bool? isRequeue = null;

      try
      {
        await _messageHandler.HandleMessageAsync(message.Item, _stoppingToken);

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
    }
  }
}
