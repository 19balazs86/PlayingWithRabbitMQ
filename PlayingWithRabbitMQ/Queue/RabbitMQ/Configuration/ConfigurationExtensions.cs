using System;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ.Configuration
{
  public static class ConfigurationExtensions
  {
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
  }
}
