using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingWithRabbitMQ.Queue.Configuration
{
  public static class ConfigurationExtensions
  {
    private static readonly IEnumerable<string> _exchangeTypes = new [] { "direct", "fanout", "topic" };

    /// <summary>
    /// Validation for BrokerFactoryConfiguration.
    /// </summary>
    public static void Validate(this BrokerFactoryConfiguration configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException(nameof(configuration));

      if (string.IsNullOrWhiteSpace(configuration.Url))
      {
        if (string.IsNullOrWhiteSpace(configuration.HostName))
          throw new ArgumentException($"{nameof(configuration.HostName)} is missing.");

        if (configuration.HostPort <= 0)
          throw new ArgumentException($"{nameof(configuration.HostPort)} is missing.");

        if (string.IsNullOrWhiteSpace(configuration.UserName))
          throw new ArgumentException($"{nameof(configuration.UserName)} is missing.");

        if (string.IsNullOrWhiteSpace(configuration.Password))
          throw new ArgumentException($"{nameof(configuration.Password)} is missing.");
      }

      if (configuration.NetworkRecoveryIntervalSeconds < 0)
        throw new ArgumentException($"{nameof(configuration.NetworkRecoveryIntervalSeconds)} can not be less than 0.");

      if (string.IsNullOrWhiteSpace(configuration.DefaultDeadLetterExchange))
        throw new ArgumentException($"{nameof(configuration.DefaultDeadLetterExchange)} is missing.");

      if (string.IsNullOrWhiteSpace(configuration.DefaultDeadLetterQueue))
        throw new ArgumentException($"{nameof(configuration.DefaultDeadLetterQueue)} is missing.");
    }

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
