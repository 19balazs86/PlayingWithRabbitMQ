using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using PlayingWithRabbitMQ.Queue.Configuration;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly ConcurrentDictionary<string, BufferBlock<object>> _queueDictionary;

    public BrokerFactory()
    {
      _queueDictionary = new ConcurrentDictionary<string, BufferBlock<object>>();
    }

    public IProducer CreateProducer(ProducerConfiguration configuration)
    {
      configuration.Validate();

      string key = $"{configuration.ExchangeName}_{configuration.RouteKey}";

      BufferBlock<object> queue = _queueDictionary.GetOrAdd(key, new BufferBlock<object>());

      return new Producer(queue.AsObserver());
    }

    public IConsumer CreateConsumer(ConsumerConfiguration configuration, Action<string> connectionShutdown = null)
    {
      configuration.Validate();

      string key = $"{configuration.ExchangeName}_{configuration.RouteKey}";

      BufferBlock<object> queue = _queueDictionary.GetOrAdd(key, new BufferBlock<object>());

      return new Consumer(queue.AsObservable(), configuration.QueueName);
    }
  }
}
