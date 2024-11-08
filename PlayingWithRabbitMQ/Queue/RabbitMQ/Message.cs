using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public class Message<T>(IChannel _channel, BasicDeliverEventArgs _eventArgs) : IMessage<T> where T : class
{
    public string RawItem => Encoding.UTF8.GetString(_eventArgs.Body.ToArray());

    public T Item => JsonSerializer.Deserialize<T>(RawItem);

    /// <summary>
    /// Acknowledge the message.
    /// </summary>
    /// <exception cref="MessageException"></exception>
    public async Task AcknowledgeAsync()
    {
        try
        {
            await _channel.BasicAckAsync(_eventArgs.DeliveryTag, false);
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
    public async Task RejectAsync(bool requeue = false)
    {
        try
        {
            // Requeue is false, send it to the dead letter queue.
            await _channel.BasicNackAsync(_eventArgs.DeliveryTag, multiple: false, requeue: requeue);
        }
        catch (Exception ex)
        {
            throw new MessageException("Failed to reject the message with BasicNack.", ex);
        }
    }
}
