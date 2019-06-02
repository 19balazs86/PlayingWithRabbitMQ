using System;
using System.IO;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.FileSystem
{
  public class Message<T> : IMessage<T> where T : class
  {
    private readonly string _messageFullPath;
    private readonly string _failedMessageFolderPath;

    private readonly Lazy<string> _lazyRawItem;

    public string RawItem => _lazyRawItem.Value;

    public T Item => JsonConvert.DeserializeObject<T>(RawItem);

    public Message(string messageFullPath, string failedMessageFolderPath)
    {
      _messageFullPath         = messageFullPath;
      _failedMessageFolderPath = failedMessageFolderPath;

      _lazyRawItem = new Lazy<string>(() => File.ReadAllText(_messageFullPath));
    }

    public void Acknowledge()
    {
      try
      {
        File.Delete(_messageFullPath);
      }
      catch (Exception ex)
      {
        throw new MessageException($"Failed to delete the file: '{_messageFullPath}'.", ex);
      }
    }

    public void Reject(bool requeue = false)
    {
      if (requeue) return; // Keep the file/message in the folder.

      string failedMsgFullPath = Path.Combine(_failedMessageFolderPath, Path.GetFileName(_messageFullPath));

      try
      {
        File.Move(_messageFullPath, failedMsgFullPath);
      }
      catch (Exception ex)
      {
        throw new MessageException($"Failed to move the file: '{_messageFullPath}'.", ex);
      }
    }
  }
}
