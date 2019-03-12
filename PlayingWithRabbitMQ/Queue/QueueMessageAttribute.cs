using System;
using System.Linq;

namespace PlayingWithRabbitMQ.Queue
{
  [AttributeUsage(AttributeTargets.Class)]
  public class QueueMessageAttribute : Attribute
  {
    private static readonly string[] _exchangeTypes = new [] { "direct", "fanout", "topic" };

    #region Properties
    /// <summary>
    /// Message be published into this exchange.
    /// </summary>
    public string ExchangeName { get; private set; }

    /// <summary>
    /// Values: direct, fanout, topic.
    /// </summary>
    public string ExchangeType { get; private set; }

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
    #endregion

    public QueueMessageAttribute(
      string exchangeName,
      string exchangeType,
      string routeKey,
      string queueName,
      string deadLetterQueue = null,
      ushort prefetchCount   = 5)
    {
      ExchangeName    = exchangeName;
      ExchangeType    = exchangeType?.ToLower();
      RouteKey        = routeKey;
      QueueName       = queueName;
      DeadLetterQueue = deadLetterQueue;
      PrefetchCount   = prefetchCount;
    }

    public void Validate()
    {
      if (string.IsNullOrWhiteSpace(ExchangeName))
        throw new ArgumentException($"{nameof(ExchangeName)} is missing.");

      if (string.IsNullOrWhiteSpace(ExchangeType))
        throw new ArgumentException($"{nameof(ExchangeType)} is missing.");

      if (!_exchangeTypes.Contains(ExchangeType))
        throw new ArgumentException($"{nameof(ExchangeType)} is wrong.");

      if (ExchangeType != "fanout" && string.IsNullOrWhiteSpace(RouteKey))
        throw new ArgumentException($"{nameof(RouteKey)} is missing.");

      if (string.IsNullOrWhiteSpace(QueueName))
        throw new ArgumentException(nameof(QueueName) + " is missing.");

      if (PrefetchCount < 0)
        throw new ArgumentOutOfRangeException(nameof(PrefetchCount) + " can not be less than 0.");
    }
  }
}
