namespace PlayingWithRabbitMQ.Queue.Configuration
{
  /// <summary>
  /// The Producer publish messages to the given exchange with the route key.
  /// The system automatically create the exchange with the given type, if it is not exists.
  /// </summary>
  public class ProducerConfiguration
  {
    /// <summary>
    /// Publish the message to this exchange.
    /// </summary>
    public string ExchangeName { get; set; }

    /// <summary>
    /// Values: direct, fanout, topic.
    /// </summary>
    public string ExchangeType { get; set; }

    /// <summary>
    /// If ExchangeType is direct or topic, you have to provide the RouteKey.
    /// </summary>
    public string RouteKey { get; set; }
  }
}