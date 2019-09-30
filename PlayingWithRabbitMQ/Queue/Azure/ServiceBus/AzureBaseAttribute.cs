using System;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus
{
  [AttributeUsage(AttributeTargets.Class)]
  public abstract class AzureBaseAttribute : Attribute
  {
    public abstract void Validate();
  }
}
