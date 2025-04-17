using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using TCGOrderManagement.OrderService.Events;
using TCGOrderManagement.OrderService.Interfaces;
using TCGOrderManagement.Shared.Configuration;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Service responsible for publishing order events to the message broker
    /// </summary>
    public class OrderEventPublisher : IOrderEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<OrderEventPublisher> _logger;
        private readonly string _exchangeName;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the OrderEventPublisher class
        /// </summary>
        /// <param name="messageBrokerSettings">The message broker configuration</param>
        /// <param name="logger">The logger</param>
        public OrderEventPublisher(MessageBrokerSettings messageBrokerSettings, ILogger<OrderEventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (messageBrokerSettings == null)
                throw new ArgumentNullException(nameof(messageBrokerSettings));
                
            _exchangeName = messageBrokerSettings.OrderExchangeName;
            
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = messageBrokerSettings.HostName,
                    UserName = messageBrokerSettings.UserName,
                    Password = messageBrokerSettings.Password,
                    VirtualHost = messageBrokerSettings.VirtualHost,
                    Port = messageBrokerSettings.Port
                };
                
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                // Declare the exchange
                _channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);
                
                _logger.LogInformation("OrderEventPublisher initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize OrderEventPublisher");
                throw;
            }
        }

        /// <summary>
        /// Publishes an order created event to the message broker
        /// </summary>
        /// <param name="orderCreatedEvent">The order created event</param>
        public async Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderCreatedEvent)
        {
            await PublishEventAsync(orderCreatedEvent, "order.created");
        }

        /// <summary>
        /// Publishes an order status changed event to the message broker
        /// </summary>
        /// <param name="orderStatusChangedEvent">The order status changed event</param>
        public async Task PublishOrderStatusChangedEventAsync(OrderStatusChangedEvent orderStatusChangedEvent)
        {
            await PublishEventAsync(orderStatusChangedEvent, "order.status.changed");
        }

        /// <summary>
        /// Publishes an order payment processed event to the message broker
        /// </summary>
        /// <param name="orderPaymentProcessedEvent">The order payment processed event</param>
        public async Task PublishOrderPaymentProcessedEventAsync(OrderPaymentProcessedEvent orderPaymentProcessedEvent)
        {
            await PublishEventAsync(orderPaymentProcessedEvent, "order.payment.processed");
        }

        /// <summary>
        /// Publishes an order shipping updated event to the message broker
        /// </summary>
        /// <param name="orderShippingUpdatedEvent">The order shipping updated event</param>
        public async Task PublishOrderShippingUpdatedEventAsync(OrderShippingUpdatedEvent orderShippingUpdatedEvent)
        {
            await PublishEventAsync(orderShippingUpdatedEvent, "order.shipping.updated");
        }

        /// <summary>
        /// Publishes an order shipped event to the message broker
        /// </summary>
        /// <param name="orderShippedEvent">The order shipped event</param>
        public async Task PublishOrderShippedEventAsync(OrderShippedEvent orderShippedEvent)
        {
            await PublishEventAsync(orderShippedEvent, "order.shipped");
        }

        /// <summary>
        /// Publishes an order cancelled event to the message broker
        /// </summary>
        /// <param name="orderCancelledEvent">The order cancelled event</param>
        public async Task PublishOrderCancelledEventAsync(OrderCancelledEvent orderCancelledEvent)
        {
            await PublishEventAsync(orderCancelledEvent, "order.cancelled");
        }

        /// <summary>
        /// Publishes an order item added event to the message broker
        /// </summary>
        /// <param name="orderItemAddedEvent">The order item added event</param>
        public async Task PublishOrderItemAddedEventAsync(OrderItemAddedEvent orderItemAddedEvent)
        {
            await PublishEventAsync(orderItemAddedEvent, "order.item.added");
        }

        /// <summary>
        /// Publishes an order item removed event to the message broker
        /// </summary>
        /// <param name="orderItemRemovedEvent">The order item removed event</param>
        public async Task PublishOrderItemRemovedEventAsync(OrderItemRemovedEvent orderItemRemovedEvent)
        {
            await PublishEventAsync(orderItemRemovedEvent, "order.item.removed");
        }

        /// <summary>
        /// Publishes an order item quantity updated event to the message broker
        /// </summary>
        /// <param name="orderItemQuantityUpdatedEvent">The order item quantity updated event</param>
        public async Task PublishOrderItemQuantityUpdatedEventAsync(OrderItemQuantityUpdatedEvent orderItemQuantityUpdatedEvent)
        {
            await PublishEventAsync(orderItemQuantityUpdatedEvent, "order.item.quantity.updated");
        }

        /// <summary>
        /// Publishes an order return processed event to the message broker
        /// </summary>
        /// <param name="orderReturnProcessedEvent">The order return processed event</param>
        public async Task PublishOrderReturnProcessedEventAsync(OrderReturnProcessedEvent orderReturnProcessedEvent)
        {
            await PublishEventAsync(orderReturnProcessedEvent, "order.return.processed");
        }

        /// <summary>
        /// Publishes an order delivered event to the message broker
        /// </summary>
        /// <param name="orderDeliveredEvent">The order delivered event</param>
        public async Task PublishOrderDeliveredEventAsync(OrderDeliveredEvent orderDeliveredEvent)
        {
            await PublishEventAsync(orderDeliveredEvent, "order.delivered");
        }

        /// <summary>
        /// Generic method to publish an event to the message broker
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="routingKey">The routing key</param>
        private async Task PublishEventAsync<T>(T @event, string routingKey) where T : OrderEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Routing key cannot be null or empty", nameof(routingKey));

            try
            {
                string message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.ContentType = "application/json";

                await Task.Run(() => _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body));

                _logger.LogInformation("Published {EventType} with ID {EventId} for order {OrderId}",
                    @event.GetType().Name, properties.MessageId, @event.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish {EventType} for order {OrderId}",
                    @event.GetType().Name, @event.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Disposes of the connection and channel
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the connection and channel
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
            }

            _disposed = true;
        }
    }
} 