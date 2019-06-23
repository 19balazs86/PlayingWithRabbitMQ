using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using StackExchange.Redis;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class Consumer<T> : IConsumer<T> where T : class
  {
    private readonly ChannelMessageQueue _channelMessageQueue;
    private readonly Subject<IMessage<T>> _subject;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(ChannelMessageQueue channelMessageQueue)
    {
      _channelMessageQueue = channelMessageQueue;

      _subject = new Subject<IMessage<T>>();

      MessageSource = _subject.AsObservable();

      _channelMessageQueue.OnMessage(onMessage);
    }

    private void onMessage(ChannelMessage channelMessage)
    {
      var message = new Message<T>(channelMessage.Message);

      _subject.OnNext(message);
    }

    public void Dispose()
    {
      _channelMessageQueue.Unsubscribe();

      _subject.Dispose();
    }
  }
}
