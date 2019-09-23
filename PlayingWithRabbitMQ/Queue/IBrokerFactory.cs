using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IBrokerFactory : IDisposable
  {
    /// <summary>
    /// Create a Producer to publish messages.
    /// </summary>
    Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class;

    /// <summary>
    /// Create a Consumer to consume messages.
    /// </summary>
    Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class;
  }
}