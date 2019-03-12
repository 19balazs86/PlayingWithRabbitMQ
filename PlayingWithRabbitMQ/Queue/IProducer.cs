using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IProducer<T> : IDisposable where T : class
  {
    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(T message);
  }
}