using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public sealed class Consumer<T> : IConsumer<T> where T : class
{
    private readonly IChannel _channel;

    private readonly Subject<IMessage<T>> _subject;

    public AsyncEventingBasicConsumer EventingConsumer { get; }

    public IObservable<IMessage<T>> MessageSource { get; }

    public Consumer(IChannel channel)
    {
        _channel = channel;

        _subject = new Subject<IMessage<T>>();

        MessageSource = _subject.AsObservable();

        EventingConsumer = new AsyncEventingBasicConsumer(channel);

        EventingConsumer.ReceivedAsync += consumerOnReceived;
    }

    private Task consumerOnReceived(object sender, BasicDeliverEventArgs deliverEventArgs)
    {
        var message = new Message<T>(_channel, deliverEventArgs);

        _subject.OnNext(message);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        EventingConsumer.ReceivedAsync -= consumerOnReceived;

        _subject.Dispose();

        _channel.Dispose();
    }
}
