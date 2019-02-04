namespace PlayingWithRabbitMQ.RabbitMQ.Configuration
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
  }
}