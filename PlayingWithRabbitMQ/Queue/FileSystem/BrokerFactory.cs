using System;
using System.Collections.Concurrent;
using System.IO;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.FileSystem
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly string _folderPath;

    private readonly ConcurrentDictionary<Type, string> _messagePathDic;
    private readonly ConcurrentDictionary<Type, string> _failedMessagePathDic;

    public BrokerFactory(string folderPath)
    {
      _folderPath = Path.GetFullPath(folderPath);

      _messagePathDic       = new ConcurrentDictionary<Type, string>();
      _failedMessagePathDic = new ConcurrentDictionary<Type, string>();
    }

    public IProducer<T> CreateProducer<T>() where T : class
      => new Producer<T>(getMessagePath<T>());

    public IConsumer<T> CreateConsumer<T>() where T : class
      => new Consumer<T>(getMessagePath<T>(), getFailedMessagePath<T>());

    private string getMessagePath<T>()
      => _messagePathDic.GetOrAdd(typeof(T), t => prepareDirectoryForMessage(t));

    private string getFailedMessagePath<T>()
      => _failedMessagePathDic.GetOrAdd(typeof(T), t => prepareDirectoryForMessage(t, "sink"));

    private string prepareDirectoryForMessage(Type msgType, string subFolder = "")
    {
      string msgFullPath = Path.Combine(_folderPath, msgType.FullName, subFolder);

      if (!Directory.Exists(msgFullPath))
      {
        try
        {
          Directory.CreateDirectory(msgFullPath);
        }
        catch (Exception ex)
        {
          throw new QueueFactoryException($"Failed to create the folder: '{msgFullPath}'.", ex);
        }
      }

      return msgFullPath;
    }

    public void Dispose()
    {
      // Empty.
    }
  }
}
