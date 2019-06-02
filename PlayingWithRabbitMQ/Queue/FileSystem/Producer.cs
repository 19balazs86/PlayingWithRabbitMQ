using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.FileSystem
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private static readonly JsonSerializer _serializer = new JsonSerializer();

    private readonly string _messageFolderPath;

    public Producer(string messageFolderPath)
      => _messageFolderPath = messageFolderPath;

    public Task PublishAsync(T message)
    {
      if (message is null)
        throw new ArgumentNullException(nameof(message));

      string fileFullPath = Path.Combine(_messageFolderPath, $"{Guid.NewGuid().ToString("N")}.json");

      try
      {
        using (var streamWriter     = new StreamWriter(fileFullPath))
        using (var jsonWriterwriter = new JsonTextWriter(streamWriter))
          _serializer.Serialize(jsonWriterwriter, message);
      }
      catch (Exception ex)
      {
        throw new ProducerException($"Failed to save/write the {typeof(T).Name} into a JSON file.", ex);
      }

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      // Nothing.
    }
  }
}
