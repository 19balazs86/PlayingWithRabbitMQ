using System.Text.Json;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class Message<T> : IMessage<T> where T : class
  {
    public string RawItem { get; private set; }

    public T Item => JsonSerializer.Deserialize<T>(RawItem);

    public Message(string message) => RawItem = message;

    // This is a pub/sub messaging system, not queuing.
    public Task AcknowledgeAsync() => Task.CompletedTask;

    // This is a pub/sub messaging system, not queuing.
    public Task RejectAsync(bool requeue = false) => Task.CompletedTask;
  }
}
