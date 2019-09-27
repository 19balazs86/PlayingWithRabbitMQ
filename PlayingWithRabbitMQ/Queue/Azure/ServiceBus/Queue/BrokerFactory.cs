using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PlayingWithRabbitMQ.Queue.Exceptions;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly string _connectionString;
    private readonly IAttributeProvider<AzureQueueAttribute> _attributeProvider;

    private readonly Lazy<ManagementClient> _lazyManagementClient;

    private readonly ConcurrentDictionary<string, ISenderClient> _senderClientsDic;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoresDic;

    public BrokerFactory(string connectionString, IAttributeProvider<AzureQueueAttribute> attributeProvider = null)
    {
      if (string.IsNullOrWhiteSpace(connectionString))
        throw new ArgumentNullException(nameof(connectionString));

      _connectionString  = connectionString;
      _attributeProvider = attributeProvider ?? new SimpleAttributeProvider<AzureQueueAttribute>();

      _lazyManagementClient = new Lazy<ManagementClient>(new ManagementClient(connectionString));

      _senderClientsDic = new ConcurrentDictionary<string, ISenderClient>();
      _semaphoresDic    = new ConcurrentDictionary<string, SemaphoreSlim>();
    }

    public async Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      try
      {
        string queueName = getAndValidateAttributeFor<T>().QueueName;

        // Try to get an ISenderClient.
        if (_senderClientsDic.TryGetValue(queueName, out ISenderClient senderClient))
          return new Producer<T>(senderClient);

        // Check whether the Queue exists otherwise create it.
        await checkQueueExistsOrCreateAsync(queueName, cancelToken);

        senderClient = _senderClientsDic.GetOrAdd(queueName, new MessageSender(_connectionString, queueName));

        return new Producer<T>(senderClient);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Producer.", ex);
      }
    }

    public async Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      try
      {
        AzureQueueAttribute queueAttribute = getAndValidateAttributeFor<T>();

        // Check whether the Queue exists otherwise create it.
        await checkQueueExistsOrCreateAsync(queueAttribute.QueueName, cancelToken);

        IReceiverClient receiverClient = new MessageReceiver(_connectionString, queueAttribute.QueueName);

        return new Consumer<T>(receiverClient, queueAttribute.MaxConcurrentCalls);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Consumer.", ex);
      }
    }

    public void Dispose()
    {
      try
      {
        if (_lazyManagementClient.IsValueCreated)
          _lazyManagementClient.Value.CloseAsync().GetAwaiter().GetResult();

        foreach (SemaphoreSlim semaphore in _semaphoresDic.Values)
          semaphore.Dispose();

        foreach (ISenderClient senderClient in _senderClientsDic.Values)
          senderClient.CloseAsync().GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Failed to dispose the BrokerFactory.");
      }
    }

    private AzureQueueAttribute getAndValidateAttributeFor<T>() where T : class
    {
      AzureQueueAttribute msgSettings = _attributeProvider.GetAttributeFor<T>();

      if (msgSettings is null)
        throw new NullReferenceException($"{nameof(AzureQueueAttribute)} is not present for the {typeof(T).Name}.");

      msgSettings.Validate();

      return msgSettings;
    }

    private async Task checkQueueExistsOrCreateAsync(string queueName, CancellationToken cancelToken)
    {
      var semaphore = _semaphoresDic.GetOrAdd(queueName, new SemaphoreSlim(1, 1));

      await semaphore.WaitAsync(cancelToken);

      try
      {
        if (!await _lazyManagementClient.Value.QueueExistsAsync(queueName, cancelToken))
          await _lazyManagementClient.Value.CreateQueueAsync(queueName, cancelToken);
      }
      finally
      {
        semaphore.Release();
      }
    }
  }
}
