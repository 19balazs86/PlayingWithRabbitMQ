using System;
using System.Reactive.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Consumer<T> : IConsumer<T> where T : class, new()
  {
    private readonly IModel _model;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(IModel model, string queueName, ushort prefetchCount = 5)
    {
      _model = model;

      _model.BasicQos(0, prefetchCount, false);

      var consumer = new EventingBasicConsumer(_model);

      MessageSource = Observable.FromEvent<EventHandler<BasicDeliverEventArgs>, BasicDeliverEventArgs>(
        // Conversion.
        action => (sender, e) => action(e),
        // Add handler / observer.
        handler =>
        {
          consumer.Received += handler;

          if (!consumer.IsRunning)
            _model.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        },
        // Remove handler / observer.
        handler =>
        {
          if (consumer.IsRunning)
            consumer.OnCancel();

          consumer.Received -= handler;
        })
        .Select(queueMessage => new Message<T>(_model, queueMessage));
    }

    public void Dispose() => _model.Dispose();
  }
}