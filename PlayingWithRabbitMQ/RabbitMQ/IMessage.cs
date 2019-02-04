namespace PlayingWithRabbitMQ.RabbitMQ
{
  public interface IMessage
  {
    string Data { get; }

    /// <summary>
    /// Deserialize the Data to the requested type of object.
    /// </summary>
    T GetDataAs<T>() where T: class, new();

    /// <summary>
    /// Acknowledge the message.
    /// </summary>
    void Acknowledge();

    /// <summary>
    /// Reject the message. It will be sent in to the dead letter queue.
    /// </summary>
    void Reject(bool requeue = false);
  }
}