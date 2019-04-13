using System;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IConsumer<T> : IDisposable where T : class
  {
    IObservable<IMessage<T>> MessageSource { get; }
  }
}