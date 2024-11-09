using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public sealed class Consumer<T> : IConsumer<T> where T : class
{
    private readonly IChannel _channel;

    public IObservable<IMessage<T>> MessageSource { get; }

    public Consumer(IChannel channel, string queueName, ushort prefetchCount = 5)
    {
        _channel = channel;

        channel.BasicQosAsync(0, prefetchCount, false).GetAwaiter().GetResult();

        // A long time ago, in the early days of my garage tinkering, the ways of the async methods were not yet embraced across the galaxy
        // The constructor declaration and IObservable worked well together
        // However, after RabbitMq major update, it adopted async methods
        // Using GetAwaiter().GetResult() feels less appealing
        // A refactored solution with IAsyncEnumerable<IMessage<T>> would be an improvement over IObservable
        // As demonstrated here: https://gist.github.com/19balazs86/30821a73aca72325984104dcd2f12f5a#file-filesystemwatcher_to_asyncenumerable-cs

        var consumer = new AsyncEventingBasicConsumer(_channel);

        MessageSource = Observable.FromEvent<AsyncEventHandler<BasicDeliverEventArgs>, BasicDeliverEventArgs>(
            // Conversion
            action => (_, eventArgs) =>
            {
                action(eventArgs);
                return Task.CompletedTask;
            },
            // Add handler / observer
            handler =>
            {
                consumer.ReceivedAsync += handler;

                if (!consumer.IsRunning && channel.IsOpen)
                {
                    // Start consuming from the RabbitMQ queue
                    _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer).GetAwaiter().GetResult();
                }
            },
            // Remove handler / observer
            handler =>
            {
                consumer.ReceivedAsync -= handler;
            })
            .Select(deliverEventArgs => new Message<T>(_channel, deliverEventArgs));
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
