using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue
{
  public class BrokerFactory : BrokerFactoryBase<AzureQueueAttribute>
  {
    public BrokerFactory(ServiceBusConfiguration configuration, IAttributeProvider<AzureQueueAttribute> attributeProvider = null)
      : base(configuration, attributeProvider)
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
        if (!_configuration.SkipManagement)
          await checkQueueExistsOrCreateAsync(queueName, cancelToken);

        senderClient = _senderClientsDic.GetOrAdd(queueName, qName =>
          new MessageSender(_configuration.ConnectionString, qName));

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
        if (!_configuration.SkipManagement)
          await checkQueueExistsOrCreateAsync(queueAttribute.QueueName, cancelToken);

        IReceiverClient receiverClient = new MessageReceiver(_configuration.ConnectionString, queueAttribute.QueueName);

        return new Consumer<T>(receiverClient, queueAttribute.MaxConcurrentCalls);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Consumer.", ex);
      }
    }

    private async Task checkQueueExistsOrCreateAsync(string queueName, CancellationToken cancelToken)
    {
      var semaphore = _semaphoresDic.GetOrAdd(queueName, _ => new SemaphoreSlim(1, 1));

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
