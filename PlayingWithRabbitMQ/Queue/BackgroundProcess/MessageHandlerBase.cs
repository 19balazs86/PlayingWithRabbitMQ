using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Configuration;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public abstract class MessageHandlerBase<TMessage> : IMessageHandler where TMessage : class, new()
  {
    public ConsumerConfiguration ConsumerConfiguration { get; private set; }

    protected MessageHandlerBase(ConsumerConfiguration consumerConfiguration)
    {
      ConsumerConfiguration = consumerConfiguration;
    }

    public abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken = default);

    public async Task HandleMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
      TMessage typedMessage;

      try
      {
        typedMessage = message.GetDataAs<TMessage>();
      }
      catch (JsonReaderException ex)
      {
        Log.Error(ex, $"Failed to deserialize the message({typeof(TMessage).Name}) with handler({GetType().Name})." + " Data: {@MessageData}.", message.Data);

        message.Reject();

        return;
      }

      try
      {
        await HandleMessageAsync(typedMessage, cancellationToken);

        message.Acknowledge();

        Log.Verbose($"Acknowledge: message({typeof(TMessage).Name}).");
      }
      catch (OperationCanceledException ex)
      {
        Log.Warning(ex, $"The operation was canceled. The message({typeof(TMessage).Name}) will be requeue.");

        message.Reject(requeue: true);
      }
      catch (Exception ex)
      {
        Log.Error(ex, $"An exception occurred during processing the message with handler({GetType().Name})." + " Data: {@MessageData}.", message.Data);

        message.Reject();
      }
    }
  }
}
