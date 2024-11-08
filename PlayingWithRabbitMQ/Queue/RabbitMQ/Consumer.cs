using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public sealed class Consumer<T> : IConsumer<T> where T : class
{
    private readonly IChannel _channel;

    private readonly Subject<IMessage<T>> _subject;

    private readonly AsyncEventingBasicConsumer _asyncEventingConsumer;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(IChannel channel, string queueName, ushort prefetchCount = 5)
    {
        _channel = channel;

        _subject = new Subject<IMessage<T>>();

        MessageSource = _subject.AsObservable();

        _asyncEventingConsumer = new AsyncEventingBasicConsumer(channel);

        _asyncEventingConsumer.ReceivedAsync += consumerOnReceived;

        _channel.BasicQosAsync(0, prefetchCount, false).GetAwaiter().GetResult();

        _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: _asyncEventingConsumer).GetAwaiter().GetResult();
    }

    private Task consumerOnReceived(object sender, BasicDeliverEventArgs deliverEventArgs)
    {
        _subject.OnNext(new Message<T>(_channel, deliverEventArgs));

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _asyncEventingConsumer.ReceivedAsync -= consumerOnReceived;

        _channel.Dispose();
    }
}
