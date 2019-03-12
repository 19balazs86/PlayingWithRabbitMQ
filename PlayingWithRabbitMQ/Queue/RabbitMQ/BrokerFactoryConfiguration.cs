using System;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class BrokerFactoryConfiguration
  {
    /// <summary>
    /// AMQP URL. HostName, Port, VirtualHost, UserName, Password are optional, if you provide the Url.
    /// </summary>
    public string Url { get; set; }

    public string HostName { get; set; }

    public int HostPort { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    public string UserName { get; set; }

    public string Password { get; set; }

    /// <summary>
    /// To enable automatic recovery, set it greater than 0.
    /// </summary>
    public int NetworkRecoveryIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Mandatory field.
    /// Dead letter exhange for rejected messages.
    /// </summary>
    public string DefaultDeadLetterExchange { get; set; }

    /// <summary>
    /// Mandatory field.
    /// Dead letter queue for rejected messages, if you do not specify it in the ConsumerConfiguration.
    /// </summary>
    public string DefaultDeadLetterQueue { get; set; }

    public static void Validate(BrokerFactoryConfiguration configuration)
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