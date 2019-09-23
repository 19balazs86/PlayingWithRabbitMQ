using System;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IBrokerFactory : IDisposable
  {
    /// <summary>
    /// Create a Producer to publish messages.
    /// </summary>
    Task<IProducer<T>> CreateProducerAsync<T>() where T : class;

    /// <summary>
    /// Create a Consumer to consume messages.
    /// </summary>
    Task<IConsumer<T>> CreateConsumerAsync<T>() where T : class;
  }
}