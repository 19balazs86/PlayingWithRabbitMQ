using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class Message<T> : IMessage<T> where T : class
  {
    private readonly IModel _model;
    private readonly BasicDeliverEventArgs _queueMessage;

    public string RawItem => Encoding.UTF8.GetString(_queueMessage.Body.ToArray());

    /// <exception cref="JsonReaderException"></exception>
    public T Item => JsonConvert.DeserializeObject<T>(RawItem);

    public Message(IModel model, BasicDeliverEventArgs queueMessage)
    {
      _model        = model;
      _queueMessage = queueMessage;
    }

    /// <summary>
    /// Acknowledge the message.
    /// </summary>
    /// <exception cref="MessageException"></exception>
    public Task AcknowledgeAsync()
    {
      try
      {
        _model.BasicAck(_queueMessage.DeliveryTag, false);

        return Task.CompletedTask;
      }
      catch (Exception ex)
      {
        throw new MessageException("Failed to acknowledge the message with BasicAck.", ex);
      }
    }

    /// <summary>
    /// Reject the message. It will be sent in to the dead letter queue.
    /// </summary>
    /// <exception cref="MessageException"></exception>
    public Task RejectAsync(bool requeue = false)
    {
      try
      {
        // Requeue is false, send it to the dead letter queue.
        _model.BasicNack(_queueMessage.DeliveryTag, multiple: false, requeue: requeue);

        return Task.CompletedTask;
      }
      catch (Exception ex)
      {
        throw new MessageException("Failed to reject the message with BasicNack.", ex);
      }
    }
  }
}