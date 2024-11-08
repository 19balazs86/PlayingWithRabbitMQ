using PlayingWithRabbitMQ.Queue.Exceptions;
using RabbitMQ.Client;
using Serilog;
using System.Collections.Concurrent;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ;

public sealed class BrokerFactory : IBrokerFactory
{
    private readonly BrokerFactoryConfiguration _factoryConfiguration;

    private readonly IAttributeProvider<MessageSettingsAttribute> _attributeProvider;

    private readonly ConnectionFactory _connectionFactory;

    private readonly Lazy<Task<IConnection>> _lazyConnection;

    private readonly ConcurrentDictionary<Type, MessageSettingsAttribute> _attributesDic;

    /// <summary>
    /// BrokerFactory constructor.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown, if the configuration is wrong.</exception>
    /// <exception cref="ArgumentException">Thrown, if the configuration is wrong.</exception>
    public BrokerFactory(
        BrokerFactoryConfiguration                   configuration,
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

        _lazyConnection = new Lazy<Task<IConnection>>(createConnection);

        _attributesDic = new ConcurrentDictionary<Type, MessageSettingsAttribute>();
    }

    /// <summary>
    /// Create Producer to publish messages.
    /// </summary>
    /// <exception cref="BrokerFactoryException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<IProducer<T>> CreateProducerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
        MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

        try
        {
            IConnection connection = await _lazyConnection.Value;

            IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancelToken);

            // Create the requested exchange.
            if (!_factoryConfiguration.SkipManagement)
            {
                await channel.ExchangeDeclareAsync(msgSettings.ExchangeName, msgSettings.ExchangeType.ToString().ToLower(), true, cancellationToken: cancelToken);
            }

            // --> Create: Producer.
            return new Producer<T>(channel, msgSettings.ExchangeName, msgSettings.RouteKey, msgSettings.DeliveryMode);
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
    public async Task<IConsumer<T>> CreateConsumerAsync<T>(CancellationToken cancelToken = default) where T : class
    {
        MessageSettingsAttribute msgSettings = getAndValidateSettingsFor<T>();

        try
        {
            IConnection connection = await _lazyConnection.Value;

            IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancelToken);

            if (!_factoryConfiguration.SkipManagement)
            {
                // --> Initialize: DeadLetterQueue and DeadLetterExchange.
                string deadLetterQueue = _factoryConfiguration.DefaultDeadLetterQueue;

                if (!string.IsNullOrWhiteSpace(msgSettings.DeadLetterQueue))
                {
                    deadLetterQueue = msgSettings.DeadLetterQueue;
                }

                await channel.QueueDeclareAsync(deadLetterQueue, true, false, false, cancellationToken: cancelToken);

                await channel.ExchangeDeclareAsync(_factoryConfiguration.DefaultDeadLetterExchange, ExchangeType.Direct.ToString().ToLower(), true, cancellationToken: cancelToken);

                await channel.QueueBindAsync(deadLetterQueue, _factoryConfiguration.DefaultDeadLetterExchange, msgSettings.QueueName, cancellationToken: cancelToken);

                // --> Initialize: The requested Queue.
                var declareArguments = new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"]    = _factoryConfiguration.DefaultDeadLetterExchange,
                    ["x-dead-letter-routing-key"] = msgSettings.QueueName
                };

                await channel.QueueDeclareAsync(msgSettings.QueueName, true, false, false, declareArguments, cancellationToken: cancelToken);

                // Create: Exchange and bind it with the queue.
                await channel.ExchangeDeclareAsync(msgSettings.ExchangeName, msgSettings.ExchangeType.ToString().ToLower(), true, cancellationToken: cancelToken);
                await channel.QueueBindAsync(msgSettings.QueueName, msgSettings.ExchangeName, msgSettings.RouteKey ?? string.Empty, cancellationToken: cancelToken);
            }

            // --> Create: Consumer.
            return new Consumer<T>(channel, msgSettings.QueueName, msgSettings.PrefetchCount);
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

    private async Task<IConnection> createConnection()
    {
        IConnection connection = await _connectionFactory.CreateConnectionAsync();

        // --> Event handlers.
        connection.ConnectionShutdownAsync += (_, eventArgs) =>
        {
            Log.Error("RabbitMQ connection is lost. {@ShutdownEventArgs}", eventArgs);

            return Task.CompletedTask;
        };

        connection.CallbackExceptionAsync += (_, eventArgs) =>
        {
            Log.Error(eventArgs.Exception, "RabbitMQ connection recovery error.");

            return Task.CompletedTask;
        };

        return connection;
    }
}
