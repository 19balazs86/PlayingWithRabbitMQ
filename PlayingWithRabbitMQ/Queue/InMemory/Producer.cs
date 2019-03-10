using System;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Producer : IProducer
  {
    private readonly IObserver<object> _observer;

    public Producer(IObserver<object> observer)
    {
      _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public void Publish(object message)
      => _observer.OnNext(message);

    public void Dispose()
    {
      // Do nothing.
    }
  }
}
