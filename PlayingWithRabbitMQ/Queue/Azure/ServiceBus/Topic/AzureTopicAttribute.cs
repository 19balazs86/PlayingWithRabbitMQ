using System;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Topic
{
  public class AzureTopicAttribute : AzureBaseAttribute
  {
    public string Topic { get; private set; }
    public string Subscription { get; private set; }
    public string RouteKey { get; private set; } // Create Subscription with filter using the RouteKey like in RabbitMQ.
    public int MaxConcurrentCalls { get; private set; }

    public AzureTopicAttribute(string topic, string subscription, string routeKey = null, int maxConcurrentCalls = 5)
    {
      Topic              = topic;
      Subscription       = subscription;
      RouteKey           = routeKey;
      MaxConcurrentCalls = maxConcurrentCalls;
    }

    public override void Validate()
    {
      if (string.IsNullOrWhiteSpace(Topic))
        throw new ArgumentException($"{nameof(Topic)} is missing.");

      if (string.IsNullOrWhiteSpace(Subscription))
        throw new ArgumentException($"{nameof(Subscription)} is missing.");

      if (MaxConcurrentCalls < 0)
        throw new ArgumentOutOfRangeException(nameof(MaxConcurrentCalls) + " can not be less than 0.");
    }
  }
}
