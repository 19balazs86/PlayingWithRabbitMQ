using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public class BrokerFactory : IBrokerFactory
  {
    private readonly BrokerFactoryConfiguration _factoryConfiguration;
    private readonly IAttributeProvider<MessageSettingsAttribute> _attributeProvider;

    private readonly IConnectionFactory _connectionFactory;

    private readonly Lazy<IConnection> _lazyConnection;

    private readonly ConcurrentDictionary<Type, MessageSettingsAttribute> _attributesDic;

    /// <summary>
    /// BrokerFactory constructor.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown, if the configuration is wrong.</exception>
    /// <exception cref="ArgumentException">Thrown, if the configuration is wrong.</exception>
    public BrokerFactory(
      BrokerFactoryConfiguration configuration,
      IAttributeProvider<MessageSettingsAttribute> attributeProvider = null)
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

      _attributeProvider = attributeProvider ?? new SimpleAttributeProvider<MessageSettingsAttribute>();

      _lazyConnection = new Lazy<IConnection>(createConnection);

      _attributesDic = new ConcurrentDictionary<Type, MessageSettingsAttribute>();
    }

    /// <summary>
    /// Create Producer to publish messages.
    /// </summary>
    /// <exception cref="BrokerFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

      try
      {
        // --> Create: Model.
        IModel model = _lazyConnection.Value.CreateModel();

        // Create the requested exchange.
        if (!_factoryConfiguration.SkipManagement)
          model.ExchangeDeclare(msgSettings.ExchangeName, msgSettings.ExchangeType.ToString().ToLower(), true);

        model.ConfirmSelect();

        // --> Create: Producer.
        IProducer<T> producer = new Producer<T>(model, msgSettings.ExchangeName, msgSettings.RouteKey, msgSettings.DeliveryMode);

        return Task.FromResult(producer);
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
    public Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
      MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

      try
      {
        // --> Create: Model.
        IModel model = _lazyConnection.Value.CreateModel();

        if (!_factoryConfiguration.SkipManagement)
        {
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
        }

        // --> Create: Consumer.
        IConsumer<T> consumer = new Consumer<T>(model, msgSettings.QueueName, msgSettings.PrefetchCount);

        return Task.FromResult(consumer);
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
      Type typeFor = typeof(T);

      if (_attributesDic.TryGetValue(typeFor, out MessageSettingsAttribute msgSettings))
        return msgSettings;

      msgSettings = _attributeProvider.GetAttributeFor<T>();

      if (msgSettings is null)
        throw new ArgumentNullException(nameof(msgSettings), $"MessageSettings is not present for the {typeof(T).Name}.");

      msgSettings.Validate();

      return _attributesDic.GetOrAdd(typeFor, msgSettings);
    }

    private IConnection createConnection()
    {
      IConnection connection = _connectionFactory.CreateConnection();

      // --> Event handlers.
      connection.ConnectionShutdown += (object sender, ShutdownEventArgs e)
        => Log.Error("RabbitMQ connection is lost. {@ShutdownEventArgs}", e);

      connection.CallbackException += (object sender, CallbackExceptionEventArgs e)
        => Log.Error(e.Exception, "RabbitMQ connection recovery error.");

      return connection;
    }
  }
}