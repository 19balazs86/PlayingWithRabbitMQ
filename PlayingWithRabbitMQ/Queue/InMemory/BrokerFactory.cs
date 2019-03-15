using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly ConcurrentDictionary<string, object> _queueDictionary
      = new ConcurrentDictionary<string, object>();

    public IProducer<T> CreateProducer<T>() where T : class
      => new Producer<T>(getQueueFor<T>().AsObserver());

    public IConsumer<T> CreateConsumer<T>(Action connectionShutdown = null) where T : class, new()
      => new Consumer<T>(getQueueFor<T>().AsObservable());

    private Subject<T> getQueueFor<T>()
      => _queueDictionary.GetOrAdd(typeof(T).FullName, new Subject<T>()) as Subject<T>;
  }
}
