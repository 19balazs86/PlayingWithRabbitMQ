using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Message<T> : IMessage<T> where T : class
  {
    public string RawItem => JsonSerializer.Serialize(Item);

    public T Item { get; private set; }

    public Message(T message)
    {
      Item = message;
    }

    public Task AcknowledgeAsync()
    {
      Log.Verbose($"InMemory - Acknowledge message({Item.GetType().Name}).");

      return Task.CompletedTask;
    }

    public Task RejectAsync(bool requeue = false)
    {
      Log.Verbose($"InMemory - Reject message({Item.GetType().Name}) with requeue = {requeue}.");

      return Task.CompletedTask;
    }
  }
}
