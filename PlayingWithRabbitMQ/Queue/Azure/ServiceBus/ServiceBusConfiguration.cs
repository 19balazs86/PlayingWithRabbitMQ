namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus
{
  public class ServiceBusConfiguration
  {
    public string ConnectionString { get; set; }

    /// <summary>
    /// BrokerFactory can create Queues, Topics, and Subscriptions if not exists.
    /// Set it true, if you do not want this management process or the connection string does not have manage claims.
    /// </summary>
    public bool SkipManagement { get; set; } = true;
  }
}
