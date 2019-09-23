using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class Message<T> : IMessage<T> where T : class
  {
    public string RawItem { get; private set; }

    public T Item => JsonConvert.DeserializeObject<T>(RawItem);

    public Message(string message) => RawItem = message;

    // This is a pub/sub messaging system, not queuing.
    public Task AcknowledgeAsync(CancellationToken cancelToken = default) => Task.CompletedTask;

    // This is a pub/sub messaging system, not queuing.
    public Task RejectAsync(bool requeue = false, CancellationToken cancelToken = default) => Task.CompletedTask;
  }
}
