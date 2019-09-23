using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly Subject<object> _subject = new Subject<object>();

    public Task<IProducer<T>> CreateProducerAsync<T>() where T : class
      => Task.FromResult<IProducer<T>>(new Producer<T>(_subject.AsObserver<T>()));

    public Task<IConsumer<T>> CreateConsumerAsync<T>() where T : class
      => Task.FromResult<IConsumer<T>>(new Consumer<T>(_subject.OfType<T>()));

    public void Dispose() => _subject.Dispose();
  }
}
