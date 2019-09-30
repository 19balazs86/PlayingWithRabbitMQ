using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue
{
  public class BrokerFactory : BrokerFactoryBase<AzureQueueAttribute>
  {
    public BrokerFactory(string connectionString, IAttributeProvider<AzureQueueAttribute> attributeProvider = null)
      : base(connectionString, attributeProvider)
    {

    }

    public override async Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
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

    public override async Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class
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
