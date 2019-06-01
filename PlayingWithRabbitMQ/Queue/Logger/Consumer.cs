using System;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.Logger
{
  public class Consumer<T> : IConsumer<T> where T : class
  {
    public IObservable<IMessage<T>> MessageSource => Observable.Empty<IMessage<T>>();

    public void Dispose()
    {
      // Empty.
    }
  }
}
