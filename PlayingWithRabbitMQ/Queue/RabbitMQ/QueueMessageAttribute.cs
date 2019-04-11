using System;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public enum ExchangeType { Direct, Fanout, Topic }

  public enum DeliveryMode { NonPersistent = 1, Persistent = 2 }

  [AttributeUsage(AttributeTargets.Class)]
  public class QueueMessageAttribute : Attribute
  {
    #region Properties
    /// <summary>
    /// Message be published into this exchange.
    /// </summary>
    public string ExchangeName { get; private set; }

    /// <summary>
    /// Values: Direct, Fanout, Topic.
    /// </summary>
    public ExchangeType ExchangeType { get; private set; }

    /// <summary>
    /// If exchangeType is direct or topic, you have to provide the RouteKey.
    /// </summary>
    public string RouteKey { get; private set; }

    /// <summary>
    /// Queue name which you want to consume.
    /// </summary>
    public string QueueName { get; private set; }

    /// <summary>
    /// Dead letter queue for rejected messages. If you do not specify it, the system use the default from the QueueFactoryConfiguration.
    /// </summary>
    public string DeadLetterQueue { get; private set; }

    /// <summary>
    /// This tells RabbitMQ not to give more than x message to a worker at a time. Bigger number means more threads.
    /// </summary>
    public ushort PrefetchCount { get; private set; }

    /// <summary>
    /// Publish the message as Persistent or NonPersistent.
    /// Messages sent as Persistent that are delivered to 'durable' queues will be logged to disk.
    /// </summary>
    public DeliveryMode DeliveryMode { get; private set; }
    #endregion

    public QueueMessageAttribute(
      string exchangeName,
      ExchangeType exchangeType,
      string routeKey,
      string queueName,
      string deadLetterQueue    = null,
      ushort prefetchCount      = 5,
      DeliveryMode deliveryMode = DeliveryMode.Persistent)
    {
      ExchangeName    = exchangeName;
      ExchangeType    = exchangeType;
      RouteKey        = routeKey;
      QueueName       = queueName;
      DeadLetterQueue = deadLetterQueue;
      PrefetchCount   = prefetchCount;
      DeliveryMode    = deliveryMode;
    }

    public void Validate()
    {
      if (string.IsNullOrWhiteSpace(ExchangeName))
        throw new ArgumentException($"{nameof(ExchangeName)} is missing.");

      if (ExchangeType != ExchangeType.Fanout && string.IsNullOrWhiteSpace(RouteKey))
        throw new ArgumentException($"{nameof(RouteKey)} is missing.");

      if (string.IsNullOrWhiteSpace(QueueName))
        throw new ArgumentException(nameof(QueueName) + " is missing.");

      if (PrefetchCount < 0)
        throw new ArgumentOutOfRangeException(nameof(PrefetchCount) + " can not be less than 0.");
    }
  }
}
