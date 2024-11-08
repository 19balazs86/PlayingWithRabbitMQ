using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using System.Text.Json;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public sealed class Producer<T>(IChannel _channel, string _exchangeName, string _routingKey, DeliveryMode _deliveryMode)
    : IProducer<T> where T : class
{
    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    private async Task publish(byte[] message, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_channel.IsClosed, "Producer is already disposed");

        if (message is null || message.Length == 0)
        {
            throw new ArgumentNullException(nameof(message));
        }

        // var props = new BasicProperties
        // {
        //     ContentType     = MediaTypeNames.Application.Json,
        //     ContentEncoding = Encoding.UTF8.WebName,
        //     DeliveryMode    = _deliveryMode == DeliveryMode.Persistent ? DeliveryModes.Persistent : DeliveryModes.Transient
        // };
        //
        // var publicationAddress = new PublicationAddress("exchangeType???", _exchangeName, _routingKey);

        try
        {
            // await _channel.BasicPublishAsync(publicationAddress, props, message, ct);

            await _channel.BasicPublishAsync(_exchangeName, _routingKey, message, ct);
        }
        catch (Exception ex)
        {
            throw new ProducerException("Failed to publish the message with BasicPublish.", ex);
        }
    }

    /// <summary>
    /// Publish a message.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ProducerException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task PublishAsync(T message, CancellationToken cancelToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        byte[] messageBytes = JsonSerializer.SerializeToUtf8Bytes(message);

        await publish(messageBytes, cancelToken);
    }

    public void Dispose() => _channel.Dispose();
}
