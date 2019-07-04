using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IProducer<T> : IDisposable where T : class
  {
    /// <summary>
    /// Publish a message.
    /// </summary>
    Task PublishAsync(T message, CancellationToken cancelToken = default);
  }
}