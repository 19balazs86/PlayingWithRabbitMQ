﻿using System;
using System.Collections.Generic;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly BrokerFactoryConfiguration _factoryConfiguration;
    private readonly IMessageSettingsProvider _messageSettingsProvider;

    private readonly IConnectionFactory _connectionFactory;

    private readonly Lazy<IConnection> _lazyConnection;

    /// <summary>
    /// BrokerFactory constructor.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown, if the configuration is wrong.</exception>
    /// <exception cref="ArgumentException">Thrown, if the configuration is wrong.</exception>
    public BrokerFactory(BrokerFactoryConfiguration configuration, IMessageSettingsProvider messageSettingsProvider = null)
    {
      BrokerFactoryConfiguration.Validate(configuration);

      if (string.IsNullOrWhiteSpace(configuration.Url))
      {
        _connectionFactory = new ConnectionFactory
        {
          HostName                 = configuration.HostName,
          Port                     = configuration.HostPort,
          VirtualHost              = configuration.VirtualHost,
          UserName                 = configuration.UserName,
          Password                 = configuration.Password,
          AutomaticRecoveryEnabled = configuration.NetworkRecoveryIntervalSeconds > 0,
          NetworkRecoveryInterval  = TimeSpan.FromSeconds(configuration.NetworkRecoveryIntervalSeconds)
        };
      }
      else
      {
        _connectionFactory = new ConnectionFactory
        {
          Uri                      = new Uri(configuration.Url),
          AutomaticRecoveryEnabled = configuration.NetworkRecoveryIntervalSeconds > 0,
          NetworkRecoveryInterval  = TimeSpan.FromSeconds(configuration.NetworkRecoveryIntervalSeconds)
        };
      }

      _factoryConfiguration = configuration;

      _messageSettingsProvider = messageSettingsProvider ?? new SimpleMessageSettingsProvider();

      _lazyConnection = new Lazy<IConnection>(createConnection);
    }

    /// <summary>
    /// Create Producer to publish messages.
    /// </summary>
    /// <exception cref="BrokerFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IProducer<T> CreateProducer<T>() where T : class
    {
      MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

      try
      {
        // --> Create: Model.
        IModel model = _lazyConnection.Value.CreateModel();
        
        // Create the requested exchange.
        model.ExchangeDeclare(msgSettings.ExchangeName, msgSettings.ExchangeType.ToString().ToLower(), true);
        model.ConfirmSelect();

        // --> Create: Producer.
        return new Producer<T>(model, msgSettings.ExchangeName, msgSettings.RouteKey, msgSettings.DeliveryMode);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Producer.", ex);
      }
    }

    /// <summary>
    /// Create Consumer to consume messages.
    /// </summary>
    /// <exception cref="BrokerFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IConsumer<T> CreateConsumer<T>() where T : class
    {
      MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

      try
      {
        // --> Create: Model.
        IModel model = _lazyConnection.Value.CreateModel();

        // --> Initialize: DeadLetterQueue and DeadLetterExchange.
        string deadLetterQueue = _factoryConfiguration.DefaultDeadLetterQueue;

        if (!string.IsNullOrWhiteSpace(msgSettings.DeadLetterQueue))
          deadLetterQueue = msgSettings.DeadLetterQueue;

        model.QueueDeclare(deadLetterQueue, true, false, false);

        model.ExchangeDeclare(_factoryConfiguration.DefaultDeadLetterExchange, ExchangeType.Direct.ToString().ToLower(), true);

        model.QueueBind(deadLetterQueue, _factoryConfiguration.DefaultDeadLetterExchange, msgSettings.QueueName);

        // --> Initialize: The requested Queue.
        Dictionary<string, object> declareArguments = new Dictionary<string, object>
        {
          ["x-dead-letter-exchange"]    = _factoryConfiguration.DefaultDeadLetterExchange,
          ["x-dead-letter-routing-key"] = msgSettings.QueueName
        };

        model.QueueDeclare(msgSettings.QueueName, true, false, false, declareArguments);

        // Create: Exchange and bind it with the queue.
        model.ExchangeDeclare(msgSettings.ExchangeName, msgSettings.ExchangeType.ToString().ToLower(), true);
        model.QueueBind(msgSettings.QueueName, msgSettings.ExchangeName, msgSettings.RouteKey ?? string.Empty);

        // --> Create: Consumer.
        return new Consumer<T>(model, msgSettings.QueueName, msgSettings.PrefetchCount);
      }
      catch (Exception ex)
      {
        throw new BrokerFactoryException("Failed to create Consumer.", ex);
      }
    }

    public void Dispose()
    {
      if (_lazyConnection.IsValueCreated)
        _lazyConnection.Value.Dispose();
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private MessageSettingsAttribute getAndValidateSettingsFor<T>() where T : class
    {
      MessageSettingsAttribute msgSettings = _messageSettingsProvider.GetMessageSettingsFor<T>();

      if (msgSettings is null)
        throw new ArgumentNullException(nameof(msgSettings), $"MessageSettings is not present for the {typeof(T).Name}.");

      msgSettings.Validate();

      return msgSettings;
    }

    private IConnection createConnection()
    {
      IConnection connection = _connectionFactory.CreateConnection();

      // --> Event handlers.
      connection.ConnectionShutdown += (object sender, ShutdownEventArgs e)
        => Log.Error("RabbitMQ connection is lost. {@ShutdownEventArgs}", e);

      connection.ConnectionRecoveryError += (object sender, ConnectionRecoveryErrorEventArgs e)
        => Log.Error(e.Exception, "RabbitMQ connection recovery error.");

      connection.RecoverySucceeded += (object sender, EventArgs e)
        => Log.Information("RabbitMQ connection recovery succeeded.");

      return connection;
    }
  }
}