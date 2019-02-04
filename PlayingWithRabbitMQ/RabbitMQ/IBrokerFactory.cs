using System;
using PlayingWithRabbitMQ.RabbitMQ.Configuration;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public interface IBrokerFactory
  {
    /// <summary>
    /// Create a Producer to publish messages.
    /// </summary>
    IProducer CreateProducer(ProducerConfiguration configuration);

    /// <summary>
    /// Create a Consumer to consume messages.
    /// </summary>
    IConsumer CreateConsumer(ConsumerConfiguration configuration, Action<string> connectionShutdown = null);
  }
}