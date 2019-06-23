using Newtonsoft.Json;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class Message<T> : IMessage<T> where T : class
  {
    public string RawItem { get; private set; }

    public T Item => JsonConvert.DeserializeObject<T>(RawItem);

    public Message(string message) => RawItem = message;

    public void Acknowledge()
    {
      // This is a pub/sub messaging system, not queuing.
    }

    public void Reject(bool requeue = false)
    {
      // This is a pub/sub messaging system, not queuing.
    }
  }
}
