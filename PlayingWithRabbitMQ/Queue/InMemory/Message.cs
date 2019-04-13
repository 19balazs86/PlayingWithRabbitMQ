using Newtonsoft.Json;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Message<T> : IMessage<T> where T : class
  {
    public string RawItem => JsonConvert.SerializeObject(Item);

    public T Item { get; private set; }

    public Message(T message)
    {
      Item = message;
    }

    public void Acknowledge()
      => Log.Verbose($"InMemory - Acknowledge message({Item.GetType().Name}).");

    public void Reject(bool requeue = false)
      => Log.Verbose($"InMemory - Reject message({Item.GetType().Name}) with requeue = {requeue}.");
  }
}
