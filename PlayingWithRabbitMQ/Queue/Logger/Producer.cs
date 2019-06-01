using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.Logger
{
  public class Producer<T> : IProducer<T> where T : class
  {
    public Task PublishAsync(T message)
    {
      string messageText = JsonConvert.SerializeObject(message);

      Log.Verbose($"{typeof(T).Name} is published. {messageText}.");

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      // Empty.
    }
  }
}
