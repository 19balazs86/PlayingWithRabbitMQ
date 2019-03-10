using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IConsumer : IDisposable
  {
    string QueueName { get; }

    IObservable<IMessage> MessageSource { get; }
  }
}