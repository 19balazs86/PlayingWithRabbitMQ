﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.Azure.ServiceBus
{
  public abstract class BrokerFactoryBase<A> : IBrokerFactory where A : AzureBaseAttribute
  {
    protected readonly ServiceBusConfiguration _configuration;
    private readonly IAttributeProvider<A> _attributeProvider;

    protected readonly Lazy<ManagementClient> _lazyManagementClient;

    protected readonly ConcurrentDictionary<string, ISenderClient> _senderClientsDic;
    protected readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoresDic;
    protected readonly ConcurrentDictionary<Type, A> _attributesDic;

    public BrokerFactoryBase(ServiceBusConfiguration configuration, IAttributeProvider<A> attributeProvider = null)
    {
      _configuration     = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _attributeProvider = attributeProvider ?? new SimpleAttributeProvider<A>();

      _lazyManagementClient = new Lazy<ManagementClient>(new ManagementClient(configuration.ConnectionString));

      _senderClientsDic = new ConcurrentDictionary<string, ISenderClient>();
      _semaphoresDic    = new ConcurrentDictionary<string, SemaphoreSlim>();
      _attributesDic    = new ConcurrentDictionary<Type, A>();
    }

    public abstract Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class;
    public abstract Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class;

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

    protected A getAndValidateAttributeFor<T>() where T : class
    {
      Type typeFor = typeof(T);

      if (_attributesDic.TryGetValue(typeFor, out A msgSettings))
        return msgSettings;

      msgSettings = _attributeProvider.GetAttributeFor<T>();

      if (msgSettings is null)
        throw new NullReferenceException($"{typeof(A).Name} is not present for the {typeof(T).Name}.");

      msgSettings.Validate();

      return _attributesDic.GetOrAdd(typeFor, msgSettings);
    }
  }
}
