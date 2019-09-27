using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue
{
  public class Message<T> : IMessage<T> where T : class
  {
    private readonly IReceiverClient _receiverClient;
    private readonly Message _message;

    public string RawItem => Encoding.UTF8.GetString(_message.Body);

    public T Item => JsonConvert.DeserializeObject<T>(RawItem);

    public Message(IReceiverClient receiverClient, Message message)
    {
      _receiverClient = receiverClient;
      _message        = message;
    }

    public async Task AcknowledgeAsync()
    {
      try
      {
        await _receiverClient.CompleteAsync(_message.SystemProperties.LockToken);
      }
      catch (Exception ex)
      {
        throw new MessageException("Failed to complete the message.", ex);
      }
    }

    public async Task RejectAsync(bool requeue = false)
    {
      try
      {
        if (requeue)
          await _receiverClient.AbandonAsync(_message.SystemProperties.LockToken);
        else
          await _receiverClient.DeadLetterAsync(_message.SystemProperties.LockToken);
      }
      catch (Exception ex)
      {
        throw new MessageException("Failed to reject the message.", ex);
      }
    }
  }
}
