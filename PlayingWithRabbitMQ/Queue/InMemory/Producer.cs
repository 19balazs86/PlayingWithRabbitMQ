using System;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly IObserver<T> _observer;

    public Producer(IObserver<T> observer)
    {
      _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public Task PublishAsync(T message)
    {
      _observer.OnNext(message);

      return Task.CompletedTask;
    }

    public void Dispose() => _observer.OnCompleted();
  }
}
