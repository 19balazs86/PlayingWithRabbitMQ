using System;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Consumer<T> : IConsumer<T> where T : class
  {
    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(IObservable<T> sourceObservable)
      => MessageSource = sourceObservable.Select(m => new Message<T>(m));

    public void Dispose()
    {
      // Do nothing.
    }
  }
}
