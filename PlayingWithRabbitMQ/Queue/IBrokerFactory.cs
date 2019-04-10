using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IBrokerFactory : IDisposable
  {
    /// <summary>
    /// Create a Producer to publish messages.
    /// </summary>
    IProducer<T> CreateProducer<T>() where T : class;

    /// <summary>
    /// Create a Consumer to consume messages.
    /// </summary>
    IConsumer<T> CreateConsumer<T>() where T : class, new();
  }
}