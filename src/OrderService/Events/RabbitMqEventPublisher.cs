using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.Shared.Messaging;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// RabbitMQ implementation of the event publisher for OrderService
    /// </summary>
    public class RabbitMqEventPublisher : IEventPublisher, IDisposable
    {
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">RabbitMQ configuration</param>
        /// <param name="logger">Logger</param>
        public RabbitMqEventPublisher(RabbitMqConfig config, ILogger<RabbitMqEventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            _exchangeName = config.ExchangeName;
            
            try
            {
                var factory = new ConnectionFactory { 
                    HostName = config.Host,
                    Port = config.Port,
                    UserName = config.Username,
                    Password = config.Password,
                    VirtualHost = config.VirtualHost
                };
                
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                // Declare a topic exchange
                _channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);
                
                _logger.LogInformation($"RabbitMQ connection established to {config.Host}:{config.Port}/{config.VirtualHost} with exchange {_exchangeName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ connection");
                throw;
            }
        }
        
        /// <inheritdoc />
        public Task PublishOrderCreatedEventAsync(OrderCreatedEvent eventData)
        {
            return PublishEventAsync("order.created", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishOrderUpdatedEventAsync(OrderUpdatedEvent eventData)
        {
            return PublishEventAsync("order.updated", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishPaymentProcessedEventAsync(PaymentProcessedEvent eventData)
        {
            return PublishEventAsync("order.payment.processed", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishInventoryReservedEventAsync(InventoryReservedEvent eventData)
        {
            return PublishEventAsync("order.inventory.reserved", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishInventoryReservationFailedEventAsync(InventoryReservationFailedEvent eventData)
        {
            return PublishEventAsync("order.inventory.reservation.failed", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishShippingRateCalculatedEventAsync(ShippingRateCalculatedEvent eventData)
        {
            return PublishEventAsync("order.shipping.rate.calculated", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishOrderCanceledEventAsync(OrderCancelledEvent eventData)
        {
            return PublishEventAsync("order.cancelled", eventData);
        }
        
        /// <inheritdoc />
        public Task PublishOrderCompletedEventAsync(OrderCompletedEvent eventData)
        {
            return PublishEventAsync("order.completed", eventData);
        }
        
        /// <summary>
        /// Generic method to publish an event to RabbitMQ
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="routingKey">Routing key for the event</param>
        /// <param name="eventData">Event data to publish</param>
        private Task PublishEventAsync<T>(string routingKey, T eventData) where T : OrderEvent
        {
            try
            {
                _logger.LogDebug($"Publishing {typeof(T).Name} event with routing key: {routingKey}");
                
                // Serialize the event data to JSON
                var message = JsonConvert.SerializeObject(eventData);
                var body = Encoding.UTF8.GetBytes(message);
                
                // Set message properties
                var properties = _channel.CreateBasicProperties();
                properties.ContentType = "application/json";
                properties.DeliveryMode = 2; // Persistent
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Headers = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "EventType", typeof(T).Name }
                };
                
                // Publish the message
                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
                
                _logger.LogInformation($"Published {typeof(T).Name} event for order {eventData.OrderId} with routing key: {routingKey}");
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing {typeof(T).Name} event with routing key: {routingKey}");
                throw;
            }
        }
        
        /// <summary>
        /// Dispose method to clean up RabbitMQ resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _channel?.Dispose();
                _connection?.Dispose();
                
                _logger.LogInformation("RabbitMQ connection closed and resources disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ resources");
            }
        }
    }
} 