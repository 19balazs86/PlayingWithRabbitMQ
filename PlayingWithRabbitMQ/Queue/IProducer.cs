using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IProducer : IDisposable
  {
    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(object message);
  }
}