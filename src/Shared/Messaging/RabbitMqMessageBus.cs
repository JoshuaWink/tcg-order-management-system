using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TCGOrderManagement.Shared.Messaging
{
    /// <summary>
    /// RabbitMQ implementation of the message bus interface
    /// </summary>
    public class RabbitMqMessageBus : IMessageBus, IDisposable
    {
        private readonly ILogger<RabbitMqMessageBus> _logger;
        private readonly RabbitMqConfig _config;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, List<object>> _eventHandlers;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the RabbitMqMessageBus
        /// </summary>
        /// <param name="config">Configuration for RabbitMQ</param>
        /// <param name="logger">Logger instance</param>
        public RabbitMqMessageBus(RabbitMqConfig config, ILogger<RabbitMqMessageBus> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventHandlers = new Dictionary<string, List<object>>();

            try
            {
                _config.Validate();

                var factory = new ConnectionFactory
                {
                    HostName = _config.Host,
                    UserName = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    Port = _config.Port,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchange
                _channel.ExchangeDeclare(
                    exchange: _config.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("RabbitMQ connection established to {Host}", _config.Host);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task PublishAsync<T>(string eventName, T message) where T : class
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Headers = new Dictionary<string, object>
                {
                    { "message_type", message.GetType().FullName }
                };

                _channel.BasicPublish(
                    exchange: _config.ExchangeName,
                    routingKey: eventName,
                    basicProperties: properties,
                    body: body);

                _logger.LogDebug("Published message of type {MessageType} to {Exchange} with routing key {RoutingKey}",
                    typeof(T).Name, _config.ExchangeName, eventName);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message of type {MessageType} to {Exchange} with routing key {RoutingKey}",
                    typeof(T).Name, _config.ExchangeName, eventName);
                throw;
            }
        }

        /// <inheritdoc />
        public void Subscribe<T>(string eventName, Func<T, Task> handler) where T : class
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                // Store the handler
                if (!_eventHandlers.ContainsKey(eventName))
                {
                    _eventHandlers[eventName] = new List<object>();
                }
                _eventHandlers[eventName].Add(handler);

                // Declare a queue with a unique name for this consumer
                var queueName = $"{eventName}_queue_{Guid.NewGuid()}";
                
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: true);

                _channel.QueueBind(
                    queue: queueName,
                    exchange: _config.ExchangeName,
                    routingKey: eventName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (sender, args) =>
                {
                    try
                    {
                        var body = args.Body.ToArray();
                        var jsonMessage = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<T>(jsonMessage);

                        await handler(message);
                        _channel.BasicAck(args.DeliveryTag, false);
                        
                        _logger.LogDebug("Message of type {MessageType} processed successfully from {Exchange} with routing key {RoutingKey}",
                            typeof(T).Name, _config.ExchangeName, eventName);
                    }
                    catch (Exception ex)
                    {
                        _channel.BasicNack(args.DeliveryTag, false, true);
                        _logger.LogError(ex, "Error processing message of type {MessageType} from {Exchange} with routing key {RoutingKey}",
                            typeof(T).Name, _config.ExchangeName, eventName);
                    }
                };

                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Subscribed to {EventName} events with queue {QueueName}", 
                    eventName, queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to {EventName} events", eventName);
                throw;
            }
        }

        /// <summary>
        /// Disposes of the RabbitMQ connection and channel
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the RabbitMQ connection and channel
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connections closed");
            }

            _disposed = true;
        }
    }
} 