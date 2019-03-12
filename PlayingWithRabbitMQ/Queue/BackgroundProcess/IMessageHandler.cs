using System.Threading;
using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public interface IMessageHandler<T> where T : class, new()
  {
    Task HandleMessageAsync(T message, CancellationToken cancellationToken = default);
  }
}
