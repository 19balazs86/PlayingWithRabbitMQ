using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus
{
  public class Consumer<T> : IConsumer<T> where T : class
  {
    private readonly IReceiverClient _receiverClient;

    private readonly Subject<IMessage<T>> _subject;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(IReceiverClient receiverClient, int maxConcurrentCalls = 5)
    {
      _receiverClient = receiverClient;

      _subject = new Subject<IMessage<T>>();

      MessageSource = _subject.AsObservable();

      var messageHandlerOptions = new MessageHandlerOptions(exceptionHandlerAsync)
      {
        MaxConcurrentCalls = maxConcurrentCalls,
        AutoComplete       = false
      };

      _receiverClient.RegisterMessageHandler(messageHandlerAsync, messageHandlerOptions);
    }

    public void Dispose()
    {
      try
      {
        _subject.Dispose();
        _receiverClient.CloseAsync().GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Failed to dispose the Consumer.");
      }
    }

    private Task messageHandlerAsync(Message receivedMessage, CancellationToken cancelToken)
    {
      Message<T> message = new Message<T>(_receiverClient, receivedMessage);

      _subject.OnNext(message);

      return Task.CompletedTask;
    }

    private static Task exceptionHandlerAsync(ExceptionReceivedEventArgs eventArgs)
    {
      if (eventArgs.Exception is OperationCanceledException)
        return Task.CompletedTask;

      ExceptionReceivedContext context = eventArgs.ExceptionReceivedContext;

      Log.Error(eventArgs.Exception, "Message handler encountered an exception with the {@Context}.",
        new { context.Action, context.Endpoint, context.EntityPath });

      return Task.CompletedTask;
    }
  }
}
