using System;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Consumer : IConsumer
  {
    public string QueueName { get; private set; }

    public IObservable<IMessage> MessageSource { get; private set; }

    public Consumer(IObservable<object> sourceObservable, string queueName)
    {
      MessageSource = sourceObservable?.Select(m => new Message(m))
        ?? throw new ArgumentNullException(nameof(sourceObservable));

      QueueName = queueName;
    }

    public void Dispose()
    {
      // Do nothing.
    }
  }
}
