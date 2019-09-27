using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly ISenderClient _senderClient;
    private readonly string _routeKey;

    public Producer(ISenderClient senderClient, string routeKey = null)
    {
      _senderClient = senderClient;
      _routeKey     = routeKey;
    }

    public async Task PublishAsync(T message, CancellationToken cancelToken = default)
    {
      try
      {
        string messageJson = JsonConvert.SerializeObject(message);

        Message msg = new Message(Encoding.UTF8.GetBytes(messageJson));

        if (!string.IsNullOrWhiteSpace(_routeKey))
          msg.UserProperties.Add("RouteKey", _routeKey);

        await _senderClient.SendAsync(msg);
      }
      catch (Exception ex)
      {
        throw new ProducerException("Failed to publish the message with SenderClient.", ex);
      }
    }

    public void Dispose()
    {
      // Do not close the ISenderClient, instead of reuse it.
    }
  }
}
