using System;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Consumer<T> : IConsumer<T> where T : class, new()
  {
    public string QueueName { get; private set; }

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(IObservable<T> sourceObservable, string queueName)
    {
      MessageSource = sourceObservable?.Select(m => new Message<T>(m));
      QueueName     = queueName;
    }

    public void Dispose()
    {
      // Do nothing.
    }
  }
}
