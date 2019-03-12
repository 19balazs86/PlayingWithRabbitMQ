using System;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly IConnection _connection;
    private readonly IModel _model;

    private readonly string _exchangeName;
    private readonly string _routingKey;

    private bool _isDisposed = false;

    public Producer(IConnection connection, IModel model, string exchangeName, string routingKey)
    {
      _connection   = connection;      
      _model        = model;      
      _exchangeName = exchangeName;
      _routingKey   = routingKey;
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    private void publish(byte[] message)
    {
      if (_isDisposed)
        throw new ObjectDisposedException("Producer is already disposed.");

      if (message is null || message.Length == 0)
        throw new ArgumentNullException(nameof(message));

      IBasicProperties props = _model.CreateBasicProperties();

      props.ContentType     = MediaTypeNames.Application.Json;
      props.ContentEncoding = Encoding.UTF8.WebName;
      props.DeliveryMode    = 2; // Messages marked as 'persistent' that are delivered to 'durable' queues will be logged to disk.

      _model.BasicPublish(_exchangeName, _routingKey, props, message);

      try
      {
        _model.WaitForConfirmsOrDie(TimeSpan.FromSeconds(1));
      }
      catch (RabbitMQClientException ex)
      {
        throw new ProducerException("Failed to publish the message.", ex);
      }
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Publish(T message)
    {
      if (message == null)
        throw new ArgumentNullException(nameof(message));

      string messageText = JsonConvert.SerializeObject(message);

      publish(Encoding.UTF8.GetBytes(messageText));
    }

    public void Dispose()
    {
      _isDisposed = true;

      if (_model.IsOpen) _model.Close();
      if (_connection.IsOpen) _connection.Close();
    }
  }
}