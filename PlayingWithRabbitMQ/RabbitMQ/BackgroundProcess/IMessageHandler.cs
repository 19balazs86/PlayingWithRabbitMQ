using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.RabbitMQ.Configuration;

namespace PlayingWithRabbitMQ.RabbitMQ.BackgroundProcess
{
  public interface IMessageHandler
  {
    ConsumerConfiguration ConsumerConfiguration { get; }

    Task HandleMessageAsync(IMessage message, CancellationToken cancellationToken = default);
  }
}
