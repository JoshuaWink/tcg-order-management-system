using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Interface for order service providing business logic for order management
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Get an order by its unique identifier
        /// </summary>
        /// <param name="id">The order's unique identifier</param>
        /// <returns>The order if found</returns>
        Task<Order> GetOrderByIdAsync(Guid id);
        
        /// <summary>
        /// Get orders for a specific customer
        /// </summary>
        /// <param name="customerId">The customer's unique identifier</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Collection of orders for the customer</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByCustomerAsync(
            Guid customerId,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="order">The order to create</param>
        /// <returns>The created order</returns>
        Task<Order> CreateOrderAsync(Order order);
        
        /// <summary>
        /// Update an existing order
        /// </summary>
        /// <param name="order">The order with updated information</param>
        /// <returns>The updated order</returns>
        Task<Order> UpdateOrderAsync(Order order);
        
        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="id">The order's unique identifier</param>
        /// <param name="reason">Reason for cancellation</param>
        /// <param name="cancelledBy">Username of who cancelled the order</param>
        /// <returns>True if cancellation was successful</returns>
        Task<bool> CancelOrderAsync(Guid id, string reason, string cancelledBy);
        
        /// <summary>
        /// Process payment for an order
        /// </summary>
        /// <param name="id">The order's unique identifier</param>
        /// <param name="paymentInfo">Payment information</param>
        /// <returns>Updated order with payment information</returns>
        Task<Order> ProcessPaymentAsync(Guid id, PaymentInfo paymentInfo);
        
        /// <summary>
        /// Update the shipping information for an order
        /// </summary>
        /// <param name="id">The order's unique identifier</param>
        /// <param name="shippingInfo">Updated shipping information</param>
        /// <returns>Updated order with shipping information</returns>
        Task<Order> UpdateShippingInfoAsync(Guid id, ShippingInfo shippingInfo);
        
        /// <summary>
        /// Update the status of an order
        /// </summary>
        /// <param name="id">The order's unique identifier</param>
        /// <param name="status">The new status</param>
        /// <param name="comment">Optional comment about the status change</param>
        /// <param name="changedBy">Username of who changed the status</param>
        /// <returns>True if the update was successful</returns>
        Task<bool> UpdateOrderStatusAsync(Guid id, OrderStatus status, string comment = null, string changedBy = null);
        
        /// <summary>
        /// Add an item to an existing order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <param name="orderItem">The item to add</param>
        /// <returns>The updated order</returns>
        Task<Order> AddOrderItemAsync(Guid orderId, OrderItem orderItem);
        
        /// <summary>
        /// Remove an item from an existing order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <param name="orderItemId">The order item's unique identifier</param>
        /// <returns>The updated order</returns>
        Task<Order> RemoveOrderItemAsync(Guid orderId, Guid orderItemId);
        
        /// <summary>
        /// Update the quantity of an item in an order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <param name="orderItemId">The order item's unique identifier</param>
        /// <param name="newQuantity">The new quantity</param>
        /// <returns>The updated order</returns>
        Task<Order> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderItemId, int newQuantity);
        
        /// <summary>
        /// Get the status history of an order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <returns>Collection of status history records</returns>
        Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(Guid orderId);
        
        /// <summary>
        /// Search for orders based on various criteria
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="customerId">Optional customer filter</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Collection of orders matching the criteria</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> SearchOrdersAsync(
            string searchTerm,
            OrderStatus? status = null,
            Guid? customerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Get orders that contain a specific item
        /// </summary>
        /// <param name="itemId">The item's unique identifier</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Collection of orders containing the item</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersContainingItemAsync(
            Guid itemId,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Process a shipment for an order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <param name="carrier">Shipping carrier</param>
        /// <param name="trackingNumber">Tracking number</param>
        /// <param name="shippedBy">Username of who processed the shipment</param>
        /// <returns>Updated order with shipment information</returns>
        Task<Order> ProcessShipmentAsync(Guid orderId, string carrier, string trackingNumber, string shippedBy);
        
        /// <summary>
        /// Process a return for an order
        /// </summary>
        /// <param name="orderId">The order's unique identifier</param>
        /// <param name="reason">Reason for return</param>
        /// <param name="processedBy">Username of who processed the return</param>
        /// <returns>Updated order with return information</returns>
        Task<Order> ProcessReturnAsync(Guid orderId, string reason, string processedBy);
    }
} 