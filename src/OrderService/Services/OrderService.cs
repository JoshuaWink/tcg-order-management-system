using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.OrderService.Events;
using TCGOrderManagement.OrderService.Interfaces;
using TCGOrderManagement.OrderService.Models;
using TCGOrderManagement.OrderService.Repositories;
using TCGOrderManagement.Shared.Exceptions;
using TCGOrderManagement.Shared.Messaging;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Implementation of the order service for processing trading card game orders
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly IInventoryServiceClient _inventoryClient;
        private readonly IPaymentServiceClient _paymentClient;
        private readonly ILogger<OrderService> _logger;
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class
        /// </summary>
        /// <param name="orderRepository">Repository for order data access</param>
        /// <param name="eventPublisher">Publisher for order events</param>
        /// <param name="inventoryClient">Client to interact with inventory service</param>
        /// <param name="paymentClient">Client to interact with payment service</param>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="messagePublisher">The message publisher</param>
        public OrderService(
            IOrderRepository orderRepository,
            IOrderEventPublisher eventPublisher,
            IInventoryServiceClient inventoryClient,
            IPaymentServiceClient paymentClient,
            ILogger<OrderService> logger,
            IMessagePublisher messagePublisher)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _inventoryClient = inventoryClient ?? throw new ArgumentNullException(nameof(inventoryClient));
            _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        /// <inheritdoc />
        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            _logger.LogInformation("Retrieving order with ID: {OrderId}", orderId);
            
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                throw new EntityNotFoundException($"Order with ID {orderId} not found");
            }

            return order;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving orders for customer with ID: {CustomerId}", customerId);
            return await _orderRepository.GetByCustomerIdAsync(customerId);
        }

        /// <inheritdoc />
        public async Task<OrderResult> CreateOrderAsync(OrderRequest orderRequest)
        {
            try
            {
                _logger.LogInformation("Creating new order for customer {CustomerId}", orderRequest.CustomerId);

                // Create a new order entity
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = orderRequest.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = orderRequest.ShippingAddress,
                    BillingAddress = orderRequest.BillingAddress ?? orderRequest.ShippingAddress,
                    Items = orderRequest.Items.Select(i => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ItemId = i.ItemId,
                        Name = i.Name,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Condition = i.Condition
                    }).ToList(),
                    SubTotal = orderRequest.Items.Sum(i => i.UnitPrice * i.Quantity),
                    PaymentStatus = PaymentStatus.Pending,
                    InventoryStatus = InventoryStatus.Pending
                };

                // Calculate totals
                order.TaxAmount = CalculateTax(order);
                order.Total = order.SubTotal + order.TaxAmount;

                // Save the order to the repository
                await _orderRepository.AddAsync(order);

                // Publish order created event
                var orderCreatedEvent = new OrderCreatedEvent(order.Id)
                {
                    CustomerId = order.CustomerId,
                    OrderItems = order.Items.Select(i => new OrderItemInfo
                    {
                        ItemId = i.ItemId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    TotalAmount = order.Total
                };

                await _messagePublisher.PublishAsync("order.created", orderCreatedEvent);

                _logger.LogInformation("Order {OrderId} created successfully", order.Id);

                // Return the result
                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = "Order created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order: {ErrorMessage}", ex.Message);
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error creating order: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> CancelOrderAsync(Guid orderId, string reason)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId} with reason: {Reason}", orderId, reason);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot cancel order {OrderId}: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Check if the order can be cancelled
                if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                {
                    _logger.LogWarning("Cannot cancel order {OrderId}: Order is already {Status}", orderId, order.Status);
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Cannot cancel order that is already {order.Status}"
                    };
                }

                // Update order status
                order.Status = OrderStatus.Cancelled;
                order.CancellationReason = reason;
                order.CancellationDate = DateTime.UtcNow;

                // Save changes
                await _orderRepository.UpdateAsync(order);

                // Publish order cancelled event
                var orderCancelledEvent = new OrderCancelledEvent(order.Id)
                {
                    Reason = reason,
                    CancellationDate = order.CancellationDate.Value,
                    RefundAmount = order.PaymentStatus == PaymentStatus.Paid ? order.Total : 0
                };

                await _messagePublisher.PublishAsync("order.cancelled", orderCancelledEvent);

                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = "Order cancelled successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}: {ErrorMessage}", orderId, ex.Message);
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error cancelling order: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string notes = null)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} status to {NewStatus}", orderId, newStatus);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot update order {OrderId}: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Check if the status transition is valid
                if (!IsValidStatusTransition(order.Status, newStatus))
                {
                    _logger.LogWarning("Invalid status transition for order {OrderId}: {CurrentStatus} -> {NewStatus}", 
                        orderId, order.Status, newStatus);
                    
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Invalid status transition: {order.Status} -> {newStatus}"
                    };
                }

                // Update the order status
                OrderStatus oldStatus = order.Status;
                order.Status = newStatus;
                order.LastUpdated = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    order.Notes = string.IsNullOrEmpty(order.Notes) 
                        ? notes 
                        : $"{order.Notes}\n{DateTime.UtcNow:g}: {notes}";
                }

                // Update specific fields based on new status
                UpdateOrderBasedOnStatus(order, newStatus);

                // Save changes
                await _orderRepository.UpdateAsync(order);

                // Publish order status changed event
                var statusChangedEvent = new OrderStatusChangedEvent(order.Id)
                {
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangeDate = order.LastUpdated.Value,
                    Reason = notes
                };

                await _messagePublisher.PublishAsync("order.status.changed", statusChangedEvent);

                _logger.LogInformation("Order {OrderId} status updated successfully to {NewStatus}", orderId, newStatus);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = $"Order status updated to {newStatus}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status: {ErrorMessage}", orderId, ex.Message);
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error updating order status: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderDetailResult> GetOrderDetailsAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving details for order {OrderId}", orderId);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return new OrderDetailResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                _logger.LogInformation("Order {OrderId} details retrieved successfully", orderId);

                return new OrderDetailResult
                {
                    Success = true,
                    Order = order
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId} details: {ErrorMessage}", orderId, ex.Message);
                return new OrderDetailResult
                {
                    Success = false,
                    Message = $"Error retrieving order details: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrdersResult> GetCustomerOrdersAsync(Guid customerId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving orders for customer {CustomerId}, page {PageNumber}, size {PageSize}", 
                    customerId, pageNumber, pageSize);

                var orders = await _orderRepository.GetByCustomerIdAsync(customerId, pageNumber, pageSize);
                var totalCount = await _orderRepository.GetCustomerOrderCountAsync(customerId);

                _logger.LogInformation("Retrieved {Count} orders for customer {CustomerId}", orders.Count, customerId);

                return new OrdersResult
                {
                    Success = true,
                    Orders = orders,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}: {ErrorMessage}", 
                    customerId, ex.Message);
                
                return new OrdersResult
                {
                    Success = false,
                    Message = $"Error retrieving customer orders: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus newStatus, string transactionReference = null)
        {
            try
            {
                _logger.LogInformation("Updating payment status for order {OrderId} to {NewStatus}", orderId, newStatus);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot update payment status for order {OrderId}: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Update payment status
                order.PaymentStatus = newStatus;
                order.LastUpdated = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(transactionReference))
                {
                    order.PaymentTransactionId = transactionReference;
                }

                // If payment is completed, update the order status if needed
                if (newStatus == PaymentStatus.Paid && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Processing;
                }

                // Save changes
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("Payment status for order {OrderId} updated successfully to {NewStatus}", 
                    orderId, newStatus);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = $"Payment status updated to {newStatus}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}: {ErrorMessage}", 
                    orderId, ex.Message);
                
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error updating payment status: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> UpdateInventoryStatusAsync(Guid orderId, InventoryStatus newStatus)
        {
            try
            {
                _logger.LogInformation("Updating inventory status for order {OrderId} to {NewStatus}", orderId, newStatus);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot update inventory status for order {OrderId}: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Update inventory status
                order.InventoryStatus = newStatus;
                order.LastUpdated = DateTime.UtcNow;

                // Update order status based on inventory status if needed
                UpdateOrderStatusBasedOnInventory(order, newStatus);

                // Save changes
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("Inventory status for order {OrderId} updated successfully to {NewStatus}", 
                    orderId, newStatus);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = $"Inventory status updated to {newStatus}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory status for order {OrderId}: {ErrorMessage}", 
                    orderId, ex.Message);
                
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error updating inventory status: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> UpdateShippingDetailsAsync(Guid orderId, string carrier, string trackingNumber, 
            decimal shippingCost, string shippingMethod, DateTime? estimatedDeliveryDate = null)
        {
            try
            {
                _logger.LogInformation("Updating shipping details for order {OrderId}", orderId);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot update shipping details for order {OrderId}: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Update shipping details
                order.ShippingCarrier = carrier;
                order.TrackingNumber = trackingNumber;
                order.ShippingCost = shippingCost;
                order.ShippingMethod = shippingMethod;
                order.EstimatedDeliveryDate = estimatedDeliveryDate;
                order.LastUpdated = DateTime.UtcNow;

                // Update total amount to include shipping cost
                order.Total = order.SubTotal + order.TaxAmount + order.ShippingCost;

                // Save changes
                await _orderRepository.UpdateAsync(order);

                // Publish shipping details updated event
                var shippingUpdatedEvent = new OrderShippingUpdatedEvent(order.Id)
                {
                    Carrier = carrier,
                    TrackingNumber = trackingNumber,
                    ShippingCost = shippingCost,
                    ShippingMethod = shippingMethod,
                    EstimatedDeliveryDate = estimatedDeliveryDate
                };

                await _messagePublisher.PublishAsync("order.shipping.updated", shippingUpdatedEvent);

                _logger.LogInformation("Shipping details for order {OrderId} updated successfully", orderId);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = "Shipping details updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping details for order {OrderId}: {ErrorMessage}", 
                    orderId, ex.Message);
                
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error updating shipping details: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> MarkOrderAsShippedAsync(Guid orderId, string trackingNumber, DateTime? estimatedDeliveryDate = null)
        {
            try
            {
                _logger.LogInformation("Marking order {OrderId} as shipped", orderId);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot mark order {OrderId} as shipped: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Check if the order can be marked as shipped
                if (order.Status != OrderStatus.Processing && order.Status != OrderStatus.ReadyForShipment)
                {
                    _logger.LogWarning("Cannot mark order {OrderId} as shipped: Invalid current status {Status}", 
                        orderId, order.Status);
                    
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Cannot mark order as shipped: Invalid current status {order.Status}"
                    };
                }

                // Update order
                order.Status = OrderStatus.Shipped;
                order.ShippingDate = DateTime.UtcNow;
                order.TrackingNumber = trackingNumber;
                order.EstimatedDeliveryDate = estimatedDeliveryDate;
                order.LastUpdated = DateTime.UtcNow;

                // Save changes
                await _orderRepository.UpdateAsync(order);

                // Publish order shipped event
                var shippedEvent = new OrderShippedEvent(order.Id)
                {
                    ShippingDate = order.ShippingDate.Value,
                    TrackingNumber = trackingNumber,
                    Carrier = order.ShippingCarrier,
                    EstimatedDeliveryDate = estimatedDeliveryDate
                };

                await _messagePublisher.PublishAsync("order.shipped", shippedEvent);

                _logger.LogInformation("Order {OrderId} marked as shipped successfully", orderId);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = "Order marked as shipped successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as shipped: {ErrorMessage}", orderId, ex.Message);
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error marking order as shipped: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrderResult> MarkOrderAsDeliveredAsync(Guid orderId, string deliveryNotes = null)
        {
            try
            {
                _logger.LogInformation("Marking order {OrderId} as delivered", orderId);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Cannot mark order {OrderId} as delivered: Order not found", orderId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Check if the order can be marked as delivered
                if (order.Status != OrderStatus.Shipped)
                {
                    _logger.LogWarning("Cannot mark order {OrderId} as delivered: Invalid current status {Status}", 
                        orderId, order.Status);
                    
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Cannot mark order as delivered: Invalid current status {order.Status}"
                    };
                }

                // Update order
                order.Status = OrderStatus.Delivered;
                order.DeliveryDate = DateTime.UtcNow;
                order.DeliveryNotes = deliveryNotes;
                order.LastUpdated = DateTime.UtcNow;

                // Save changes
                await _orderRepository.UpdateAsync(order);

                // Publish order delivered event
                var deliveredEvent = new OrderDeliveredEvent(order.Id)
                {
                    DeliveryDate = order.DeliveryDate.Value,
                    DeliveryNotes = deliveryNotes
                };

                await _messagePublisher.PublishAsync("order.delivered", deliveredEvent);

                _logger.LogInformation("Order {OrderId} marked as delivered successfully", orderId);

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.Id,
                    Message = "Order marked as delivered successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as delivered: {ErrorMessage}", orderId, ex.Message);
                return new OrderResult
                {
                    Success = false,
                    Message = $"Error marking order as delivered: {ex.Message}"
                };
            }
        }

        #region Private Methods

        private decimal CalculateTax(Order order)
        {
            // This is a simplified tax calculation
            // In a real system, this would likely involve calling a tax service 
            // with consideration for shipping address, tax exemptions, etc.
            const decimal taxRate = 0.0825m; // 8.25% tax rate
            return order.SubTotal * taxRate;
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define valid transitions
            switch (currentStatus)
            {
                case OrderStatus.Pending:
                    return newStatus == OrderStatus.Processing || 
                           newStatus == OrderStatus.Cancelled;
                
                case OrderStatus.Processing:
                    return newStatus == OrderStatus.ReadyForShipment || 
                           newStatus == OrderStatus.Cancelled || 
                           newStatus == OrderStatus.OnHold;
                
                case OrderStatus.ReadyForShipment:
                    return newStatus == OrderStatus.Shipped || 
                           newStatus == OrderStatus.Cancelled || 
                           newStatus == OrderStatus.OnHold;
                
                case OrderStatus.Shipped:
                    return newStatus == OrderStatus.Delivered;
                
                case OrderStatus.OnHold:
                    return newStatus == OrderStatus.Processing || 
                           newStatus == OrderStatus.Cancelled;
                
                case OrderStatus.Cancelled:
                    return false; // Cannot transition from cancelled
                
                case OrderStatus.Delivered:
                    return false; // Cannot transition from delivered
                
                default:
                    return false;
            }
        }

        private void UpdateOrderBasedOnStatus(Order order, OrderStatus newStatus)
        {
            switch (newStatus)
            {
                case OrderStatus.ReadyForShipment:
                    order.PackingDate = DateTime.UtcNow;
                    break;
                
                case OrderStatus.Shipped:
                    order.ShippingDate = DateTime.UtcNow;
                    break;
                
                case OrderStatus.Delivered:
                    order.DeliveryDate = DateTime.UtcNow;
                    break;
                
                case OrderStatus.Cancelled:
                    order.CancellationDate = DateTime.UtcNow;
                    break;
            }
        }

        private void UpdateOrderStatusBasedOnInventory(Order order, InventoryStatus newInventoryStatus)
        {
            // If inventory allocation fails, put the order on hold
            if (newInventoryStatus == InventoryStatus.Failed && order.Status == OrderStatus.Processing)
            {
                order.Status = OrderStatus.OnHold;
                
                // Add a note about the inventory issue
                string note = $"{DateTime.UtcNow:g}: Order placed on hold due to inventory issues.";
                order.Notes = string.IsNullOrEmpty(order.Notes) 
                    ? note 
                    : $"{order.Notes}\n{note}";
            }
            
            // If inventory is reserved and payment is complete, move to next stage if needed
            else if (newInventoryStatus == InventoryStatus.Reserved && 
                     order.PaymentStatus == PaymentStatus.Paid && 
                     order.Status == OrderStatus.Processing)
            {
                order.Status = OrderStatus.ReadyForShipment;
            }
        }

        #endregion
    }
} 