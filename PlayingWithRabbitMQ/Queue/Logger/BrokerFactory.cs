namespace PlayingWithRabbitMQ.Queue.Logger
{
  public class BrokerFactory : IBrokerFactory
  {
    public IProducer<T> CreateProducer<T>() where T : class
      => new Producer<T>();

    public IConsumer<T> CreateConsumer<T>() where T : class
      => new Consumer<T>();

    public void Dispose()
    {
      // Empty.
    }
  }
}
