using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IConsumer<T> : IDisposable where T : class, new()
  {
    string QueueName { get; }

    IObservable<IMessage<T>> MessageSource { get; }
  }
}