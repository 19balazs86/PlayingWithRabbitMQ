using System;
using System.Collections.Generic;
using PlayingWithRabbitMQ.RabbitMQ.Configuration;
using PlayingWithRabbitMQ.RabbitMQ.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlayingWithRabbitMQ.RabbitMQ
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly BrokerFactoryConfiguration _factoryConfiguration;

    private readonly IConnectionFactory _connectionFactory;

    /// <summary>
    /// BrokerFactory constructor.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown, if the configuration is wrong.</exception>
    /// <exception cref="ArgumentException">Thrown, if the configuration is wrong.</exception>
    public BrokerFactory(BrokerFactoryConfiguration configuration)
    {
      configuration.Validate();

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
    }

    /// <summary>
    /// Create Producer to publish messages.
    /// </summary>
    /// <exception cref="QueueFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public IProducer CreateProducer(ProducerConfiguration configuration)
    {
      configuration.Validate();

      try
      {
        IConnection connection = _connectionFactory.CreateConnection();
        IModel model           = connection.CreateModel();
        
        // Create the requested exchange.
        model.ExchangeDeclare(configuration.ExchangeName, configuration.ExchangeType, true);
        model.ConfirmSelect();

        return new Producer(connection, model, configuration.ExchangeName, configuration.RouteKey);
      }
      catch (Exception ex) when (ex is BrokerUnreachableException || ex is RabbitMQClientException)
      {
        throw new QueueFactoryException("Failed to create Producer.", ex);
      }
    }

    /// <summary>
    /// Create Consumer to consume messages.
    /// </summary>
    /// <exception cref="QueueFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public IConsumer CreateConsumer(ConsumerConfiguration configuration, Action<string> connectionShutdown = null)
    {
      configuration.Validate();

      try
      {
        // --> Create: Connection + Model.
        IConnection connection = _connectionFactory.CreateConnection();
        IModel model           = connection.CreateModel();

        // --> Initialize: DeadLetterQueue and DeadLetterExchange.
        string deadLetterQueue = _factoryConfiguration.DefaultDeadLetterQueue;

        if (!string.IsNullOrWhiteSpace(configuration.DeadLetterQueue))
          deadLetterQueue = configuration.DeadLetterQueue;

        model.QueueDeclare(deadLetterQueue, true, false, false);

        model.ExchangeDeclare(_factoryConfiguration.DefaultDeadLetterExchange, "direct", true);

        model.QueueBind(deadLetterQueue, _factoryConfiguration.DefaultDeadLetterExchange, configuration.QueueName);

        // --> Initialize: The requested Queue.
        Dictionary<string, object> declareArguments = new Dictionary<string, object>
        {
          ["x-dead-letter-exchange"]    = _factoryConfiguration.DefaultDeadLetterExchange,
          ["x-dead-letter-routing-key"] = configuration.QueueName
        };

        model.QueueDeclare(configuration.QueueName, true, false, false, declareArguments);

        if (!string.IsNullOrWhiteSpace(configuration.ExchangeName))
          model.QueueBind(configuration.QueueName, configuration.ExchangeName, configuration.RouteKey ?? string.Empty);

        // --> Create: Consumer.
        return new Consumer(connection, model, configuration.QueueName, configuration.PrefetchCount, connectionShutdown);
      }
      catch (Exception ex) when (ex is BrokerUnreachableException || ex is RabbitMQClientException)
      {
        throw new QueueFactoryException("Failed to create Consumer.", ex);
      }
    }
  }
}