using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Configuration;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public interface IMessageHandler
  {
    ConsumerConfiguration ConsumerConfiguration { get; }

    Task HandleMessageAsync(IMessage message, CancellationToken cancellationToken = default);
  }
}
