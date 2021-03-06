﻿using System;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Exceptions;
using StackExchange.Redis;

namespace PlayingWithRabbitMQ.Queue.Redis
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
    private readonly Lazy<ISubscriber> _lazySubscriber;

    private ConnectionMultiplexer _connection => _lazyConnection.Value;
    private ISubscriber _subscriber => _lazySubscriber.Value;

    public BrokerFactory(string connString = "localhost:6379")
    {
      _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connString));

      _lazySubscriber = new Lazy<ISubscriber>(() => _connection.GetSubscriber());
    }

    public Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
      => Task.FromResult<IProducer<T>>(new Producer<T>(_subscriber));

    public Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      try
      {
        return Task.FromResult<IConsumer<T>>(new Consumer<T>(_subscriber.Subscribe(typeof(T).FullName)));
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Redis Consumer.", ex);
      }
    }

    public void Dispose()
    {
      if (_lazyConnection.IsValueCreated)
        _lazyConnection.Value.Dispose();
    }
  }
}
