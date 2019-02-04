using System;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public interface IProducer : IDisposable
  {
    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(byte[] message, string contentType);

    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(string messageText, string contentType);

    /// <summary>
    /// Publish a message.
    /// </summary>
    void Publish(object message);
  }
}