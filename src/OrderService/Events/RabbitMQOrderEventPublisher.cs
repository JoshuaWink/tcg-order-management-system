using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using TCGOrderManagement.Shared.Infrastructure.Messaging;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// RabbitMQ implementation of the order event publisher
    /// </summary>
    public class RabbitMQOrderEventPublisher : IOrderEventPublisher
    {
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQOrderEventPublisher> _logger;
        private const string EXCHANGE_NAME = "order_events";

        /// <summary>
        /// Initializes a new instance of the RabbitMQOrderEventPublisher class
        /// </summary>
        /// <param name="connectionFactory">RabbitMQ connection factory</param>
        /// <param name="logger">Logger</param>
        public RabbitMQOrderEventPublisher(
            IRabbitMQConnectionFactory connectionFactory,
            ILogger<RabbitMQOrderEventPublisher> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            EnsureExchangeExists();
        }

        /// <summary>
        /// Ensures the order events exchange exists
        /// </summary>
        private void EnsureExchangeExists()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.ExchangeDeclare(
                    exchange: EXCHANGE_NAME,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure order events exchange exists");
                throw;
            }
        }

        /// <summary>
        /// Publishes an order created event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderCreatedEventAsync(OrderCreatedEvent @event)
        {
            return PublishEventAsync(@event, "order.created");
        }

        /// <summary>
        /// Publishes an order status changed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderStatusChangedEventAsync(OrderStatusChangedEvent @event)
        {
            return PublishEventAsync(@event, "order.status.changed");
        }

        /// <summary>
        /// Publishes an order payment processed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderPaymentProcessedEventAsync(OrderPaymentProcessedEvent @event)
        {
            return PublishEventAsync(@event, "order.payment.processed");
        }

        /// <summary>
        /// Publishes an order shipping updated event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderShippingUpdatedEventAsync(OrderShippingUpdatedEvent @event)
        {
            return PublishEventAsync(@event, "order.shipping.updated");
        }

        /// <summary>
        /// Publishes an order shipped event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderShippedEventAsync(OrderShippedEvent @event)
        {
            return PublishEventAsync(@event, "order.shipped");
        }

        /// <summary>
        /// Publishes an order cancelled event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderCancelledEventAsync(OrderCancelledEvent @event)
        {
            return PublishEventAsync(@event, "order.cancelled");
        }

        /// <summary>
        /// Publishes an order item added event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderItemAddedEventAsync(OrderItemAddedEvent @event)
        {
            return PublishEventAsync(@event, "order.item.added");
        }

        /// <summary>
        /// Publishes an order item removed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderItemRemovedEventAsync(OrderItemRemovedEvent @event)
        {
            return PublishEventAsync(@event, "order.item.removed");
        }

        /// <summary>
        /// Publishes an order item quantity updated event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderItemQuantityUpdatedEventAsync(OrderItemQuantityUpdatedEvent @event)
        {
            return PublishEventAsync(@event, "order.item.quantity.updated");
        }

        /// <summary>
        /// Publishes an order return processed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderReturnProcessedEventAsync(OrderReturnProcessedEvent @event)
        {
            return PublishEventAsync(@event, "order.return.processed");
        }

        /// <summary>
        /// Publishes an order delivered event
        /// </summary>
        /// <param name="event">The event to publish</param>
        public Task PublishOrderDeliveredEventAsync(OrderDeliveredEvent @event)
        {
            return PublishEventAsync(@event, "order.delivered");
        }

        /// <summary>
        /// Publishes a generic order event
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="event">The event to publish</param>
        public Task PublishEventAsync<T>(T @event) where T : OrderEvent
        {
            string routingKey = DeriveRoutingKeyFromEventType(typeof(T));
            return PublishEventAsync(@event, routingKey);
        }

        /// <summary>
        /// Derives a routing key from the event type
        /// </summary>
        /// <param name="eventType">Type of the event</param>
        /// <returns>The routing key</returns>
        private string DeriveRoutingKeyFromEventType(Type eventType)
        {
            string typeName = eventType.Name;
            
            // Remove "Event" suffix if present
            if (typeName.EndsWith("Event", StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Substring(0, typeName.Length - 5);
            }
            
            // Convert to snake_case and prepend "order."
            string routingKey = "order." + ToSnakeCase(typeName);
            return routingKey;
        }

        /// <summary>
        /// Converts PascalCase to snake_case
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <returns>The snake case text</returns>
        private string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            
            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('.');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Publishes an event with the specified routing key
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="routingKey">The routing key</param>
        private Task PublishEventAsync<T>(T @event, string routingKey) where T : OrderEvent
        {
            try
            {
                string message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                using var connection = _connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = @event.EventId.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.ContentType = "application/json";
                properties.Type = @event.GetType().Name;

                channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published {EventType} with ID {EventId} for Order {OrderId}",
                    @event.GetType().Name, @event.EventId, @event.OrderId);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish {EventType} with ID {EventId} for Order {OrderId}",
                    @event.GetType().Name, @event.EventId, @event.OrderId);
                throw;
            }
        }
    }
} 