using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingWithRabbitMQ.Queue.Configuration
{
  public static class ConfigurationExtensions
  {
    private static readonly IEnumerable<string> _exchangeTypes = new [] { "direct", "fanout", "topic" };

    /// <summary>
    /// Validation for ProducerConfiguration.
    /// </summary>
    public static void Validate(this ProducerConfiguration configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException(nameof(configuration));

      if (string.IsNullOrWhiteSpace(configuration.ExchangeName))
        throw new ArgumentException($"{nameof(configuration.ExchangeName)} is missing.");

      configuration.ExchangeType = configuration.ExchangeType?.ToLower() ??
        throw new ArgumentException($"{nameof(configuration.ExchangeType)} is missing.");

      if (!_exchangeTypes.Contains(configuration.ExchangeType))
        throw new ArgumentException($"{nameof(configuration.ExchangeType)} is wrong.");

      if (configuration.ExchangeType != "fanout" && string.IsNullOrWhiteSpace(configuration.RouteKey))
        throw new ArgumentException($"{nameof(configuration.RouteKey)} is missing.");
    }

    /// <summary>
    /// Validation for ConsumerConfiguration.
    /// </summary>
    public static void Validate(this ConsumerConfiguration configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException(nameof(configuration));

      if (string.IsNullOrWhiteSpace(configuration.QueueName))
        throw new ArgumentException(nameof(configuration.QueueName) + " is missing.");

      if (configuration.PrefetchCount < 0)
        throw new ArgumentException(nameof(configuration.PrefetchCount) + " can not be less than 0.");
    }
  }
}
