using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.Shared.Models.Items;
using TCGOrderManagement.InventoryService.Models;

namespace TCGOrderManagement.InventoryService.Services
{
    /// <summary>
    /// Service interface for inventory management operations
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets a paginated list of available inventory items
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="searchTerm">Optional search term for name or description</param>
        /// <returns>Paginated inventory result</returns>
        Task<InventoryPagedResult> GetInventoryAsync(int page = 1, int pageSize = 20, string category = null, string searchTerm = null);
        
        /// <summary>
        /// Gets detailed information for a specific inventory item
        /// </summary>
        /// <param name="itemId">ID of the item to retrieve</param>
        /// <returns>Item details or null if not found</returns>
        Task<InventoryItem> GetItemByIdAsync(Guid itemId);
        
        /// <summary>
        /// Adds a new inventory item
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>Result with the created item ID and success status</returns>
        Task<InventoryOperationResult> AddInventoryItemAsync(InventoryItem item);
        
        /// <summary>
        /// Updates an existing inventory item
        /// </summary>
        /// <param name="itemId">ID of the item to update</param>
        /// <param name="item">Updated item data</param>
        /// <param name="sellerId">ID of the seller performing the update (for authorization)</param>
        /// <returns>Result with success status and message</returns>
        Task<InventoryOperationResult> UpdateInventoryItemAsync(Guid itemId, InventoryItem item, Guid sellerId);
        
        /// <summary>
        /// Deletes an inventory item
        /// </summary>
        /// <param name="itemId">ID of the item to delete</param>
        /// <param name="sellerId">ID of the seller performing the deletion (for authorization)</param>
        /// <returns>Result with success status and message</returns>
        Task<InventoryOperationResult> DeleteInventoryItemAsync(Guid itemId, Guid sellerId);
        
        /// <summary>
        /// Checks if a seller owns a particular inventory item
        /// </summary>
        /// <param name="itemId">ID of the item to check</param>
        /// <param name="sellerId">ID of the seller</param>
        /// <returns>True if the seller owns the item, false otherwise</returns>
        Task<bool> SellerOwnsItemAsync(Guid itemId, Guid sellerId);
        
        /// <summary>
        /// Reserves inventory for an order
        /// </summary>
        /// <param name="items">List of items and quantities to reserve</param>
        /// <param name="orderId">Order ID the reservation is for</param>
        /// <param name="expiryMinutes">Minutes until the reservation expires</param>
        /// <returns>Result with success status and unavailable items if any</returns>
        Task<InventoryReservationResult> ReserveInventoryAsync(List<OrderItemRequest> items, Guid orderId, int expiryMinutes = 30);
        
        /// <summary>
        /// Confirms an inventory reservation (e.g., after payment)
        /// </summary>
        /// <param name="orderId">Order ID the reservation is for</param>
        /// <returns>Result with success status</returns>
        Task<InventoryOperationResult> ConfirmReservationAsync(Guid orderId);
        
        /// <summary>
        /// Releases a previously made reservation
        /// </summary>
        /// <param name="orderId">Order ID the reservation is for</param>
        /// <returns>Result with success status</returns>
        Task<InventoryOperationResult> ReleaseReservationAsync(Guid orderId);
    }
} 