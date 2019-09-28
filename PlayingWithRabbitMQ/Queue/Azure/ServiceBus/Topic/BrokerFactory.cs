using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PlayingWithRabbitMQ.Queue.Exceptions;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Topic
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly string _connectionString;
    private readonly IAttributeProvider<AzureTopicAttribute> _attributeProvider;

    private readonly Lazy<ManagementClient> _lazyManagementClient;

    private readonly ConcurrentDictionary<string, ISenderClient> _senderClientsDic;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoresDic;

    public BrokerFactory(string connectionString, IAttributeProvider<AzureTopicAttribute> attributeProvider = null)
    {
      if (string.IsNullOrWhiteSpace(connectionString))
        throw new ArgumentNullException(nameof(connectionString));

      _connectionString  = connectionString;
      _attributeProvider = attributeProvider ?? new SimpleAttributeProvider<AzureTopicAttribute>();

      _lazyManagementClient = new Lazy<ManagementClient>(new ManagementClient(connectionString));

      _senderClientsDic = new ConcurrentDictionary<string, ISenderClient>();
      _semaphoresDic    = new ConcurrentDictionary<string, SemaphoreSlim>();
    }

    public async Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      try
      {
        AzureTopicAttribute topicAttribute = getAndValidateAttributeFor<T>();

        // Try to get an ISenderClient.
        if (_senderClientsDic.TryGetValue(topicAttribute.Topic, out ISenderClient senderClient))
          return new Producer<T>(senderClient, topicAttribute.RouteKey);

        // Check whether the Topic exists otherwise create it.
        await checkTopicExistsOrCreateAsync(topicAttribute.Topic, cancelToken);

        senderClient = _senderClientsDic.GetOrAdd(topicAttribute.Topic, new MessageSender(_connectionString, topicAttribute.Topic));

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
        AzureTopicAttribute topicAttribute = getAndValidateAttributeFor<T>();

        // Check whether the Queue exists otherwise create it.
        await checkTopicExistsOrCreateAsync(topicAttribute.Topic, cancelToken);

        // Check whether the Subscription exists otherwise create it.
        await checkSubscriptionExistsOrCreateAsync(topicAttribute.Topic, topicAttribute.Subscription, cancelToken);

        //string subscriptionPath = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);
        //IMessageReceiver receiverClient = new MessageReceiver(connectionString, subscriptionPath);

        SubscriptionClient receiverClient
          = new SubscriptionClient(_connectionString, topicAttribute.Topic, topicAttribute.Subscription);

        // Check whether the Filter for RouteKey exists otherwise create it.
        await checkSubscriptionFiltersAsync(receiverClient, topicAttribute);

        return new Consumer<T>(receiverClient, topicAttribute.MaxConcurrentCalls);
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

    private AzureTopicAttribute getAndValidateAttributeFor<T>() where T : class
    {
      AzureTopicAttribute msgSettings = _attributeProvider.GetAttributeFor<T>();

      if (msgSettings is null)
        throw new NullReferenceException($"{nameof(AzureTopicAttribute)} is not present for the {typeof(T).Name}.");

      msgSettings.Validate();

      return msgSettings;
    }

    private async Task checkTopicExistsOrCreateAsync(string topic, CancellationToken cancelToken)
    {
      var semaphore = _semaphoresDic.GetOrAdd(topic, new SemaphoreSlim(1, 1));

      await semaphore.WaitAsync(cancelToken);

      try
      {
        if (!await _lazyManagementClient.Value.TopicExistsAsync(topic, cancelToken))
          await _lazyManagementClient.Value.CreateTopicAsync(topic, cancelToken);
      }
      finally
      {
        semaphore.Release();
      }
    }

    private async Task checkSubscriptionExistsOrCreateAsync(string topic, string subscription, CancellationToken cancelToken)
    {
      if (!await _lazyManagementClient.Value.SubscriptionExistsAsync(topic, subscription, cancelToken))
        await _lazyManagementClient.Value.CreateSubscriptionAsync(topic, subscription);
    }

    private async Task checkSubscriptionFiltersAsync(SubscriptionClient subscriptionClient, AzureTopicAttribute topicAttribute)
    {
      IEnumerable<RuleDescription> rules = await subscriptionClient.GetRulesAsync();

      // Delete default rule.
      if (rules.Any(r => r.Name == RuleDescription.DefaultRuleName))
        await subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName);

      string ruleName = "RouteKey";

      if (!rules.Any(r => r.Name == ruleName))
      {
        var filter = new SqlFilter($"{ruleName}='{topicAttribute.RouteKey}'");

        // Add rule for RouteKey.
        await subscriptionClient.AddRuleAsync(ruleName, filter);
      }
    }
  }
}
