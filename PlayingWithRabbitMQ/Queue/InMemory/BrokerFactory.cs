using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks.Dataflow;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly ConcurrentDictionary<string, object> _queueDictionary;

    public BrokerFactory()
    {
      _queueDictionary = new ConcurrentDictionary<string, object>();
    }

    public IProducer<T> CreateProducer<T>() where T : class
    {
      Subject<T> queue = getQueueFor<T>(out var queueName);

      return new Producer<T>(queue.AsObserver());
    }

    public IConsumer<T> CreateConsumer<T>(Action<string> connectionShutdown = null) where T : class, new()
    {
      Subject<T> queue = getQueueFor<T>(out var queueName);

      return new Consumer<T>(queue.AsObservable(), queueName);
    }

    private Subject<T> getQueueFor<T>(out string queueName)
    {
      QueueMessageAttribute queueMessageAttr = typeof(T).GetCustomAttribute<QueueMessageAttribute>();

      if (queueMessageAttr is null)
        throw new ArgumentNullException($"QueueMessageAttribute is not present in the {typeof(T).Name}.");

      queueMessageAttr.Validate();

      queueName = queueMessageAttr.QueueName;

      string key = $"{queueMessageAttr.ExchangeName}_{queueMessageAttr.RouteKey}";

      // You can use BufferBlock, if that is suitable for you.
      // Just for test purpose the Subject can be enough.
      return _queueDictionary.GetOrAdd(key, new Subject<T>()) as Subject<T>;
    }
  }
}
