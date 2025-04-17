using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.Shared.Repositories
{
    /// <summary>
    /// Interface for order repository providing data access operations for customer orders
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Get an order by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the order</param>
        /// <returns>The order if found, null otherwise</returns>
        Task<Order> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get orders by customer ID
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of orders for the customer</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetByCustomerIdAsync(
            Guid customerId,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Get orders by status
        /// </summary>
        /// <param name="status">The order status to filter by</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of orders with the specified status</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetByStatusAsync(
            OrderStatus status,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Get orders created within a date range
        /// </summary>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of orders created within the date range</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Search for orders based on various criteria
        /// </summary>
        /// <param name="searchTerm">Search term to match against order fields</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="customerId">Optional customer filter</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of orders matching the search criteria</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> SearchAsync(
            string searchTerm,
            OrderStatus? status = null,
            Guid? customerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Add a new order to the repository
        /// </summary>
        /// <param name="order">The order to add</param>
        /// <returns>The added order with any generated fields</returns>
        Task<Order> AddAsync(Order order);
        
        /// <summary>
        /// Update an existing order
        /// </summary>
        /// <param name="order">The order to update</param>
        /// <returns>The updated order</returns>
        Task<Order> UpdateAsync(Order order);
        
        /// <summary>
        /// Update the status of an order
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <param name="status">The new status</param>
        /// <param name="comment">Optional comment about the status change</param>
        /// <param name="changedBy">Username of who changed the status</param>
        /// <returns>True if the update was successful</returns>
        Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, string comment = null, string changedBy = null);
        
        /// <summary>
        /// Get the status history of an order
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <returns>Collection of status history records for the order</returns>
        Task<IEnumerable<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId);
        
        /// <summary>
        /// Get orders that contain a specific item
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of orders containing the item</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersContainingItemAsync(
            Guid itemId,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Delete an order (typically soft delete)
        /// </summary>
        /// <param name="id">The ID of the order to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Check if an order exists
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <returns>True if the order exists</returns>
        Task<bool> ExistsAsync(Guid id);
    }
} 