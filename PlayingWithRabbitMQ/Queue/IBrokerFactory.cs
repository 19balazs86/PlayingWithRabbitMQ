using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IBrokerFactory
  {
    /// <summary>
    /// Create a Producer to publish messages.
    /// </summary>
    IProducer<T> CreateProducer<T>() where T : class;

    /// <summary>
    /// Create a Consumer to consume messages.
    /// </summary>
    IConsumer<T> CreateConsumer<T>(Action connectionShutdown = null) where T : class, new();
  }
}