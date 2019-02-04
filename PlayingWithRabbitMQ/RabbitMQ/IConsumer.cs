using System;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public interface IConsumer : IDisposable
  {
    string QueueName { get; }

    IObservable<IMessage> MessageSource { get; }
  }
}