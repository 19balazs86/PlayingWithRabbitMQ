using System;
using System.Collections.Generic;
using System.Reflection;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
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
    }

    /// <summary>
    /// Create Producer to publish messages.
    /// </summary>
    /// <exception cref="QueueFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IProducer<T> CreateProducer<T>() where T : class
    {
      QueueMessageAttribute queueMessageAttr = getAndValidateAttributeFor<T>();

      try
      {
        // --> Create: Connection + Model.
        IConnection connection = _connectionFactory.CreateConnection();
        IModel model           = connection.CreateModel();
        
        // Create the requested exchange.
        model.ExchangeDeclare(queueMessageAttr.ExchangeName, queueMessageAttr.ExchangeType.ToString().ToLower(), true);
        model.ConfirmSelect();

        // --> Create: Producer.
        return new Producer<T>(connection, model, queueMessageAttr.ExchangeName, queueMessageAttr.RouteKey);
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
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IConsumer<T> CreateConsumer<T>() where T : class, new()
    {
      QueueMessageAttribute queueMessageAttr = getAndValidateAttributeFor<T>();

      try
      {
        // --> Create: Connection + Model.
        IConnection connection = _connectionFactory.CreateConnection();
        IModel model           = connection.CreateModel();

        // --> Initialize: DeadLetterQueue and DeadLetterExchange.
        string deadLetterQueue = _factoryConfiguration.DefaultDeadLetterQueue;

        if (!string.IsNullOrWhiteSpace(queueMessageAttr.DeadLetterQueue))
          deadLetterQueue = queueMessageAttr.DeadLetterQueue;

        model.QueueDeclare(deadLetterQueue, true, false, false);

        model.ExchangeDeclare(_factoryConfiguration.DefaultDeadLetterExchange, "direct", true);

        model.QueueBind(deadLetterQueue, _factoryConfiguration.DefaultDeadLetterExchange, queueMessageAttr.QueueName);

        // --> Initialize: The requested Queue.
        Dictionary<string, object> declareArguments = new Dictionary<string, object>
        {
          ["x-dead-letter-exchange"]    = _factoryConfiguration.DefaultDeadLetterExchange,
          ["x-dead-letter-routing-key"] = queueMessageAttr.QueueName
        };

        model.QueueDeclare(queueMessageAttr.QueueName, true, false, false, declareArguments);

        // Create: Exchange and bind it with the queue.
        model.ExchangeDeclare(queueMessageAttr.ExchangeName, queueMessageAttr.ExchangeType.ToString().ToLower(), true);
        model.QueueBind(queueMessageAttr.QueueName, queueMessageAttr.ExchangeName, queueMessageAttr.RouteKey ?? string.Empty);

        // --> Create: Consumer.
        return new Consumer<T>(connection, model, queueMessageAttr.QueueName, queueMessageAttr.PrefetchCount);
      }
      catch (Exception ex) when (ex is BrokerUnreachableException || ex is RabbitMQClientException)
      {
        throw new QueueFactoryException("Failed to create Consumer.", ex);
      }
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static QueueMessageAttribute getAndValidateAttributeFor<T>()
    {
      QueueMessageAttribute queueMessageAttr = typeof(T).GetCustomAttribute<QueueMessageAttribute>();

      if (queueMessageAttr is null)
        throw new ArgumentNullException(nameof(queueMessageAttr), $"QueueMessageAttribute is not present in the {typeof(T).Name}.");

      queueMessageAttr.Validate();

      return queueMessageAttr;
    }
  }
}