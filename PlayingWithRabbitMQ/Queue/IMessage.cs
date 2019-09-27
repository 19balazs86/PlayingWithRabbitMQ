using System.Threading.Tasks;

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
    Task AcknowledgeAsync();

    /// <summary>
    /// Reject the message. It will be sent in to the dead letter queue.
    /// </summary>
    Task RejectAsync(bool requeue = false);
  }
}