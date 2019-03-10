using System.Text;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Message : IMessage
  {
    private readonly IModel _model;
    private readonly BasicDeliverEventArgs _queueMessage;

    public Message(IModel model, BasicDeliverEventArgs queueMessage)
    {
      _model        = model;
      _queueMessage = queueMessage;
    }

    public string Data => Encoding.UTF8.GetString(_queueMessage.Body);

    /// <summary>
    /// Deserialize the Data to the requested type of object.
    /// </summary>
    /// <exception cref="JsonReaderException"></exception>
    public T GetDataAs<T>() where T : class, new()
      => JsonConvert.DeserializeObject<T>(Data);

    /// <summary>
    /// Acknowledge the message.
    /// </summary>
    /// <exception cref="MessageException"></exception>
    public void Acknowledge()
    {
      try
      {
        _model.BasicAck(_queueMessage.DeliveryTag, false);
      }
      catch (RabbitMQClientException ex)
      {
        throw new MessageException("Failed to acknowledge the message.", ex);
      }
    }

    /// <summary>
    /// Reject the message. It will be sent in to the dead letter queue.
    /// </summary>
    /// <exception cref="MessageException"></exception>
    public void Reject(bool requeue = false)
    {
      try
      {
        // Requeue is false, send it to the dead letter queue.
        _model.BasicNack(_queueMessage.DeliveryTag, multiple: false, requeue: requeue);
      }
      catch (RabbitMQClientException ex)
      {
        throw new MessageException("Failed to reject the message.", ex);
      }
    }
  }
}