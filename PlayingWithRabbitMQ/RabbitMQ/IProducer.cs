using System;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public interface IProducer : IDisposable
  {
    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(object message);
  }
}