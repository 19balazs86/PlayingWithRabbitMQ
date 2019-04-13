namespace PlayingWithRabbitMQ.Queue
{
  public interface IMessage<T> where T : class
  {
    string RawItem { get; }

    /// <summary>
    /// Deserialize the Data to the requested type of object.
    /// </summary>
    T Item { get; }

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