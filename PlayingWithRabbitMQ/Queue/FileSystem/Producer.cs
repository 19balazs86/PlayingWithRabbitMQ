using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.FileSystem
{
  public class Producer<T> : IProducer<T> where T : class
  {
    private readonly string _messageFolderPath;

    public Producer(string messageFolderPath)
      => _messageFolderPath = messageFolderPath;

    public async Task PublishAsync(T message, CancellationToken cancelToken = default)
    {
      if (message is null)
        throw new ArgumentNullException(nameof(message));

      string fileFullPath = Path.Combine(_messageFolderPath, $"{Guid.NewGuid().ToString("N")}.json");

      try
      {
        using FileStream fileStream = File.OpenWrite(fileFullPath);

        await JsonSerializer.SerializeAsync(fileStream, message);
      }
      catch (Exception ex)
      {
        throw new ProducerException($"Failed to save/write the {typeof(T).Name} into a JSON file.", ex);
      }
    }

    public void Dispose()
    {
      // Nothing.
    }
  }
}
