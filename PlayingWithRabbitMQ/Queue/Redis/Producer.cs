using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Exceptions;
using StackExchange.Redis;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly ISubscriber _subscriber;
    private readonly string _channel;

    public Producer(ISubscriber subscriber)
    {
      _subscriber = subscriber;

      _channel = typeof(T).FullName;
    }

    public async Task PublishAsync(T message, CancellationToken cancelToken = default)
    {
      if (message is null)
        throw new ArgumentNullException(nameof(message));

      try
      {
        string messageText = JsonSerializer.Serialize(message);

        await _subscriber.PublishAsync(_channel, messageText);
      }
      catch (Exception ex)
      {
        throw new ProducerException("Failed to publish a message to the Redis channel.", ex);
      }
    }

    public void Dispose()
    {
      // Do nothing.
    }
  }
}
