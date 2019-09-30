using System;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue
{
  public class AzureQueueAttribute : AzureBaseAttribute
  {
    public string QueueName { get; private set; }
    public int MaxConcurrentCalls { get; private set; }

    public AzureQueueAttribute(string queueName, int maxConcurrentCalls = 5)
    {
      QueueName          = queueName;
      MaxConcurrentCalls = maxConcurrentCalls;
    }

    public override void Validate()
    {
      if (string.IsNullOrWhiteSpace(QueueName))
        throw new ArgumentException($"{nameof(QueueName)} is missing.");

      if (MaxConcurrentCalls < 0)
        throw new ArgumentOutOfRangeException(nameof(MaxConcurrentCalls) + " can not be less than 0.");
    }
  }
}
