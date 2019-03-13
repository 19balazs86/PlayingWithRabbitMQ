using System;
using System.Reactive.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Consumer<T> : IConsumer<T> where T : class, new()
  {
    private readonly IConnection _connection;
    private readonly IModel _model;

    private readonly Action _connectionShutdown;

    private readonly EventingBasicConsumer _consumer;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(
      IConnection connection,
      IModel model,
      string queueName,
      ushort prefetchCount = 5,
      Action connectionShutdown = null)
    {
      _connection = connection;
      _model      = model;

      _connectionShutdown = connectionShutdown;

      _model.BasicQos(0, prefetchCount, false);

      _consumer = new EventingBasicConsumer(_model);

      _connection.ConnectionShutdown += connectionShutdownHandler;

      MessageSource = Observable.FromEvent<EventHandler<BasicDeliverEventArgs>, BasicDeliverEventArgs>(
        // Conversion.
        action => (sender, e) => action(e),
        // Add handler / observer.
        handler =>
        {
          _consumer.Received += handler;

          if (!_consumer.IsRunning)
            _model.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
        },
        // Remove handler / observer.
        handler =>
        {
          if (_consumer.IsRunning)
            _consumer.OnCancel();

          _consumer.Received -= handler;
        })
        .Select(queueMessage => new Message<T>(_model, queueMessage));
    }

    public void Dispose()
    {
      _connection.ConnectionShutdown -= connectionShutdownHandler;

      if (_model.IsOpen)      _model.Close();
      if (_connection.IsOpen) _connection.Close();
    }

    private void connectionShutdownHandler(object sender, ShutdownEventArgs e)
      => _connectionShutdown?.Invoke();
  }
}