using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.OrderService.Events;
using TCGOrderManagement.OrderService.Repositories.Interfaces;
using TCGOrderManagement.OrderService.Services.Interfaces;
using TCGOrderManagement.Shared.Messaging;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Handles events related to the order processing flow
    /// </summary>
    public class OrderEventHandler : IOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderEventHandler> _logger;
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEventHandler"/> class
        /// </summary>
        /// <param name="orderRepository">The order repository</param>
        /// <param name="logger">The logger</param>
        /// <param name="messagePublisher">The message publisher</param>
        public OrderEventHandler(
            IOrderRepository orderRepository,
            ILogger<OrderEventHandler> logger,
            IMessagePublisher messagePublisher)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        /// <inheritdoc />
        public async Task HandlePaymentProcessedAsync(PaymentProcessedEvent paymentEvent)
        {
            _logger.LogInformation("Handling payment processed event for order {OrderId}. Success: {Success}", 
                paymentEvent.OrderId, paymentEvent.Success);

            var order = await _orderRepository.GetByIdAsync(paymentEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when handling payment processed event", paymentEvent.OrderId);
                return;
            }

            if (paymentEvent.Success)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaymentTransactionId = paymentEvent.TransactionReference;
                order.PaymentMethod = paymentEvent.PaymentMethod;
                order.PaymentDate = paymentEvent.Timestamp;

                // If order is in Pending status, move it to Processing
                if (order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Processing;
                    
                    // Add note about payment
                    string note = $"{DateTime.UtcNow:g}: Payment processed successfully via {paymentEvent.PaymentMethod}. Transaction: {paymentEvent.TransactionReference}";
                    order.Notes = string.IsNullOrEmpty(order.Notes) 
                        ? note 
                        : $"{order.Notes}\n{note}";
                }
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Failed;
                
                // Add note about payment failure
                string note = $"{DateTime.UtcNow:g}: Payment failed: {paymentEvent.FailureReason}";
                order.Notes = string.IsNullOrEmpty(order.Notes) 
                    ? note 
                    : $"{order.Notes}\n{note}";
            }

            order.LastUpdated = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Updated order {OrderId} payment status to {PaymentStatus}", 
                order.Id, order.PaymentStatus);
        }

        /// <inheritdoc />
        public async Task HandleInventoryReservedAsync(InventoryReservedEvent inventoryEvent)
        {
            _logger.LogInformation("Handling inventory reserved event for order {OrderId}", inventoryEvent.OrderId);

            var order = await _orderRepository.GetByIdAsync(inventoryEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when handling inventory reserved event", inventoryEvent.OrderId);
                return;
            }

            order.InventoryStatus = InventoryStatus.Reserved;
            order.InventoryReservationExpiry = inventoryEvent.ReservationExpiryDate;
            order.LastUpdated = DateTime.UtcNow;

            // If payment is already processed, we can move to ReadyForShipment
            if (order.PaymentStatus == PaymentStatus.Paid && order.Status == OrderStatus.Processing)
            {
                order.Status = OrderStatus.ReadyForShipment;
                
                // Add note about inventory
                string note = $"{DateTime.UtcNow:g}: Inventory reserved successfully. Order ready for shipment.";
                order.Notes = string.IsNullOrEmpty(order.Notes) 
                    ? note 
                    : $"{order.Notes}\n{note}";
            }

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Updated order {OrderId} inventory status to Reserved", order.Id);
        }

        /// <inheritdoc />
        public async Task HandleInventoryReservationFailedAsync(InventoryReservationFailedEvent inventoryEvent)
        {
            _logger.LogInformation("Handling inventory reservation failed event for order {OrderId}: {Reason}", 
                inventoryEvent.OrderId, inventoryEvent.FailureReason);

            var order = await _orderRepository.GetByIdAsync(inventoryEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when handling inventory reservation failed event", 
                    inventoryEvent.OrderId);
                return;
            }

            order.InventoryStatus = InventoryStatus.Failed;
            order.LastUpdated = DateTime.UtcNow;

            // Put the order on hold
            order.Status = OrderStatus.OnHold;

            // Add note about inventory failure
            var unavailableItems = inventoryEvent.UnavailableItems;
            string itemDetails = string.Empty;
            if (unavailableItems != null && unavailableItems.Count > 0)
            {
                foreach (var item in unavailableItems)
                {
                    itemDetails += $"\n  - Item {item.ItemId}: Requested {item.RequestedQuantity}, Available {item.AvailableQuantity}";
                }
            }

            string note = $"{DateTime.UtcNow:g}: Inventory reservation failed: {inventoryEvent.FailureReason}{itemDetails}";
            order.Notes = string.IsNullOrEmpty(order.Notes) 
                ? note 
                : $"{order.Notes}\n{note}";

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Updated order {OrderId} inventory status to Failed and placed on hold", order.Id);
        }

        /// <inheritdoc />
        public async Task HandleShippingRateCalculatedAsync(ShippingRateCalculatedEvent shippingEvent)
        {
            _logger.LogInformation("Handling shipping rate calculated event for order {OrderId}", shippingEvent.OrderId);

            var order = await _orderRepository.GetByIdAsync(shippingEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when handling shipping rate calculated event", 
                    shippingEvent.OrderId);
                return;
            }

            // Update shipping details
            order.ShippingCost = shippingEvent.ShippingCost;
            order.ShippingMethod = shippingEvent.ShippingMethod;
            order.EstimatedDeliveryDate = shippingEvent.EstimatedDeliveryDate;
            order.ShippingCarrier = shippingEvent.Carrier;
            
            // If tracking number is provided, it might mean the order is being shipped
            if (!string.IsNullOrEmpty(shippingEvent.TrackingNumber))
            {
                order.TrackingNumber = shippingEvent.TrackingNumber;
                
                // Only change to Shipped if it was in the right state
                if (order.Status == OrderStatus.ReadyForShipment || order.Status == OrderStatus.Processing)
                {
                    order.Status = OrderStatus.Shipped;
                    order.ShippingDate = DateTime.UtcNow;
                    
                    // Add shipping note
                    string note = $"{DateTime.UtcNow:g}: Order shipped via {shippingEvent.Carrier}. " +
                        $"Tracking: {shippingEvent.TrackingNumber}";
                    
                    order.Notes = string.IsNullOrEmpty(order.Notes) 
                        ? note 
                        : $"{order.Notes}\n{note}";
                    
                    // Publish order shipped event
                    var shippedEvent = new OrderShippedEvent(order.Id)
                    {
                        TrackingNumber = shippingEvent.TrackingNumber,
                        Carrier = shippingEvent.Carrier,
                        ShippedDate = DateTime.UtcNow,
                        EstimatedDeliveryDate = shippingEvent.EstimatedDeliveryDate
                    };
                    
                    await _messagePublisher.PublishAsync("order.shipped", shippedEvent);
                }
            }

            // Update total with shipping cost
            order.Total = order.SubTotal + order.TaxAmount + order.ShippingCost;
            order.LastUpdated = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Updated order {OrderId} with shipping information", order.Id);
        }
    }
} 