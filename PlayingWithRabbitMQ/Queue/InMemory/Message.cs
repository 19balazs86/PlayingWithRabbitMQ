using Newtonsoft.Json;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class Message : IMessage
  {
    private readonly object _message;

    public string Data
      => JsonConvert.SerializeObject(_message);

    public Message(object message)
    {
      _message = message;
    }

    public T GetDataAs<T>() where T : class, new()
      => _message as T;

    public void Acknowledge()
      => Log.Verbose($"InMemory - Acknowledge message({_message.GetType().Name}).");

    public void Reject(bool requeue = false)
      => Log.Verbose($"InMemory - Reject message({_message.GetType().Name}) with requeue = {requeue}.");
  }
}
