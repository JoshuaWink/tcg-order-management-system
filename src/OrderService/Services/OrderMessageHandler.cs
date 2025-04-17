using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.OrderService.Events;
using TCGOrderManagement.OrderService.Services.Interfaces;
using TCGOrderManagement.Shared.Messaging;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Handles incoming messages from the message broker and routes them to the appropriate event handler
    /// </summary>
    public class OrderMessageHandler : IMessageHandler
    {
        private readonly IOrderEventHandler _eventHandler;
        private readonly ILogger<OrderMessageHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMessageHandler"/> class
        /// </summary>
        /// <param name="eventHandler">The order event handler</param>
        /// <param name="logger">The logger</param>
        public OrderMessageHandler(IOrderEventHandler eventHandler, ILogger<OrderMessageHandler> logger)
        {
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task HandleMessageAsync(string routingKey, string message)
        {
            try
            {
                _logger.LogInformation("Received message with routing key {RoutingKey}", routingKey);

                switch (routingKey)
                {
                    case "payment.processed":
                        await HandlePaymentProcessedAsync(message);
                        break;
                        
                    case "inventory.reserved":
                        await HandleInventoryReservedAsync(message);
                        break;
                        
                    case "inventory.reservation.failed":
                        await HandleInventoryReservationFailedAsync(message);
                        break;
                        
                    case "shipping.rate.calculated":
                        await HandleShippingRateCalculatedAsync(message);
                        break;
                        
                    default:
                        _logger.LogWarning("Unhandled routing key: {RoutingKey}", routingKey);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message with routing key {RoutingKey}: {ErrorMessage}", 
                    routingKey, ex.Message);
                // Consider implementing a dead letter queue or retry mechanism here
                throw;
            }
        }

        private async Task HandlePaymentProcessedAsync(string message)
        {
            try
            {
                var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message);
                if (paymentEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize payment processed event");
                    return;
                }

                await _eventHandler.HandlePaymentProcessedAsync(paymentEvent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing payment processed event: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task HandleInventoryReservedAsync(string message)
        {
            try
            {
                var inventoryEvent = JsonSerializer.Deserialize<InventoryReservedEvent>(message);
                if (inventoryEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize inventory reserved event");
                    return;
                }

                await _eventHandler.HandleInventoryReservedAsync(inventoryEvent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing inventory reserved event: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task HandleInventoryReservationFailedAsync(string message)
        {
            try
            {
                var inventoryEvent = JsonSerializer.Deserialize<InventoryReservationFailedEvent>(message);
                if (inventoryEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize inventory reservation failed event");
                    return;
                }

                await _eventHandler.HandleInventoryReservationFailedAsync(inventoryEvent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing inventory reservation failed event: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task HandleShippingRateCalculatedAsync(string message)
        {
            try
            {
                var shippingEvent = JsonSerializer.Deserialize<ShippingRateCalculatedEvent>(message);
                if (shippingEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize shipping rate calculated event");
                    return;
                }

                await _eventHandler.HandleShippingRateCalculatedAsync(shippingEvent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing shipping rate calculated event: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
} 