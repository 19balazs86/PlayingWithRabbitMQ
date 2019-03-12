using System;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly IObserver<T> _observer;

    public Producer(IObserver<T> observer)
    {
      _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public void Publish(T message) => _observer.OnNext(message);

    public void Dispose() => _observer.OnCompleted();
  }
}
