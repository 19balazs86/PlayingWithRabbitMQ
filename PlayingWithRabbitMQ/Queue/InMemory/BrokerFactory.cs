using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;

namespace PlayingWithRabbitMQ.Queue.InMemory
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly Subject<object> _subject = new Subject<object>();

    public IProducer<T> CreateProducer<T>() where T : class
      => new Producer<T>(_subject.AsObserver<T>());

    public IConsumer<T> CreateConsumer<T>() where T : class, new()
      => new Consumer<T>(_subject.OfType<T>().AsObservable());
  }
}
