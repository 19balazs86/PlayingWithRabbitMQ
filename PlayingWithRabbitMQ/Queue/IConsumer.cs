using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IConsumer<T> : IDisposable where T : class, new()
  {
    IObservable<IMessage<T>> MessageSource { get; }
  }
}