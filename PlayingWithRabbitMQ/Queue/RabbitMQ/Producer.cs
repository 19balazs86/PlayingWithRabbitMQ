using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly IModel _model;

    private readonly string _exchangeName;
    private readonly string _routingKey;
    private readonly DeliveryMode _deliveryMode;

    public Producer(IModel model, string exchangeName, string routingKey, DeliveryMode deliveryMode)
    {
      _model        = model;
      _exchangeName = exchangeName;
      _routingKey   = routingKey;
      _deliveryMode = deliveryMode;
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    private void publish(byte[] message)
    {
      if (_model.IsClosed)
        throw new ObjectDisposedException("Producer is already disposed.");

      if (message is null || message.Length == 0)
        throw new ArgumentNullException(nameof(message));

      IBasicProperties props = _model.CreateBasicProperties();

      props.ContentType     = MediaTypeNames.Application.Json;
      props.ContentEncoding = Encoding.UTF8.WebName;
      props.DeliveryMode    = (byte)_deliveryMode;

      try
      {
        _model.BasicPublish(_exchangeName, _routingKey, props, message);

        _model.WaitForConfirmsOrDie(TimeSpan.FromSeconds(1));
      }
      catch (Exception ex)
      {
        throw new ProducerException("Failed to publish the message with BasicPublish.", ex);
      }
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task PublishAsync(T message, CancellationToken cancelToken = default)
    {
      if (message is null)
        throw new ArgumentNullException(nameof(message));

      using var memoryStream = new MemoryStream();

      await JsonSerializer.SerializeAsync(memoryStream, message);

      publish(memoryStream.ToArray());
    }

    public void Dispose() => _model.Dispose();
  }
}