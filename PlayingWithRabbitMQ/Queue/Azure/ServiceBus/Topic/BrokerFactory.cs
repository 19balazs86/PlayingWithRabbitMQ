using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using PlayingWithRabbitMQ.Queue.Exceptions;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Topic
{
  public class BrokerFactory : BrokerFactoryBase<AzureTopicAttribute>
  {
    public BrokerFactory(ServiceBusConfiguration configuration, IAttributeProvider<AzureTopicAttribute> attributeProvider = null)
      : base(configuration, attributeProvider)
    {

    }

    public override async Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      try
      {
        AzureTopicAttribute topicAttribute = getAndValidateAttributeFor<T>();

        // Try to get an ISenderClient.
        if (_senderClientsDic.TryGetValue(topicAttribute.Topic, out ISenderClient senderClient))
          return new Producer<T>(senderClient, topicAttribute.RouteKey);

        // Check whether the Topic exists otherwise create it.
        if (!_configuration.SkipManagement)
          await checkTopicExistsOrCreateAsync(topicAttribute.Topic, cancelToken);

        senderClient = _senderClientsDic.GetOrAdd(topicAttribute.Topic,
          new MessageSender(_configuration.ConnectionString, topicAttribute.Topic));

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
        AzureTopicAttribute topicAttribute = getAndValidateAttributeFor<T>();

        // Check whether the Queue exists otherwise create it.
        if (!_configuration.SkipManagement)
          await checkTopicExistsOrCreateAsync(topicAttribute.Topic, cancelToken);

        // Check whether the Subscription exists otherwise create it.
        if (!_configuration.SkipManagement)
          await checkSubscriptionExistsOrCreateAsync(topicAttribute.Topic, topicAttribute.Subscription, cancelToken);

        //string subscriptionPath = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);
        //IMessageReceiver receiverClient = new MessageReceiver(connectionString, subscriptionPath);

        SubscriptionClient receiverClient
          = new SubscriptionClient(_configuration.ConnectionString, topicAttribute.Topic, topicAttribute.Subscription);

        // Check whether the Filter for RouteKey exists otherwise create it.
        if (!_configuration.SkipManagement)
          await checkSubscriptionFiltersAsync(receiverClient, topicAttribute);

        return new Consumer<T>(receiverClient, topicAttribute.MaxConcurrentCalls);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Consumer.", ex);
      }
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
        await _lazyManagementClient.Value.CreateSubscriptionAsync(topic, subscription, cancelToken);
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
