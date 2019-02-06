using System;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.RabbitMQ.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public class Producer : IProducer
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
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Publish(byte[] message, string contentType)
    {
      if (_isDisposed)
        throw new ObjectDisposedException("Producer is already disposed.");

      if (message is null || message.Length == 0)
        throw new ArgumentNullException(nameof(message));

      if (string.IsNullOrWhiteSpace(contentType))
        throw new ArgumentException("ContentType can not be empty.");

      IBasicProperties props = _model.CreateBasicProperties();

      props.ContentType     = contentType;
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
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Publish(string messageText, string contentType = MediaTypeNames.Application.Json)
    {
      if (string.IsNullOrWhiteSpace(messageText))
        throw new ArgumentException("Message text can not be empty.");

      Publish(Encoding.UTF8.GetBytes(messageText), contentType);
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Publish(object message)
    {
      if (message is null)
        throw new ArgumentNullException(nameof(message));

      Publish(JsonConvert.SerializeObject(message), MediaTypeNames.Application.Json);
    }

    public void Dispose()
    {
      _isDisposed = true;

      if (_model.IsOpen) _model.Close();
      if (_connection.IsOpen) _connection.Close();
    }
  }
}