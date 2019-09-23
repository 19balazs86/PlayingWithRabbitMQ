using System.Threading.Tasks;

namespace PlayingWithRabbitMQ.Queue.Logger
{
  public class BrokerFactory : IBrokerFactory
  {
    public Task<IProducer<T>> CreateProducerAsync<T>() where T : class
      => Task.FromResult<IProducer<T>>(new Producer<T>());

    public Task<IConsumer<T>> CreateConsumerAsync<T>() where T : class
      => Task.FromResult<IConsumer<T>>(new Consumer<T>());

    public void Dispose()
    {
      // Empty.
    }
  }
}
