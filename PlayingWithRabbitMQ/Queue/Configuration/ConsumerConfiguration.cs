namespace PlayingWithRabbitMQ.Queue.Configuration
{
  /// <summary>
  /// The Consumer can consume the given queue.
  /// The system automatically make the binding between the exchange and the queue with the route key.
  /// </summary>
  public class ConsumerConfiguration
  {
    /// <summary>
    /// Queue name which you want to consume.
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// Can be empty, but should be the same like in the ProducerConfiguration.
    /// </summary>
    public string RouteKey { get; set; }

    /// <summary>
    /// Can be empty, but should be the same like in the ProducerConfiguration.
    /// </summary>
    public string ExchangeName { get; set; }

    /// <summary>
    /// Dead letter queue for rejected messages.
    /// If you do not specify it, the system use take default from the QueueFactoryConfiguration.
    /// </summary>
    public string DeadLetterQueue { get; set; }

    /// <summary>
    /// This tells RabbitMQ not to give more than x message to a worker at a time.
    /// Bigger number, more thread.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 2;
  }
}