using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Repositories
{
    /// <summary>
    /// Interface for inventory repository operations
    /// </summary>
    public interface IInventoryRepository
    {
        /// <summary>
        /// Gets inventory items based on specified criteria
        /// </summary>
        /// <param name="sellerId">Optional seller ID filter</param>
        /// <param name="itemIds">Optional list of specific item IDs to retrieve</param>
        /// <param name="cardName">Optional card name filter</param>
        /// <param name="setName">Optional set name filter</param>
        /// <param name="minCondition">Optional minimum condition filter</param>
        /// <returns>A collection of inventory items matching the criteria</returns>
        Task<IEnumerable<InventoryItem>> GetInventoryAsync(
            Guid? sellerId = null,
            IEnumerable<Guid> itemIds = null,
            string cardName = null,
            string setName = null,
            ItemCondition? minCondition = null);

        /// <summary>
        /// Adds a new inventory item
        /// </summary>
        /// <param name="item">The inventory item to add</param>
        /// <returns>The added inventory item with generated ID</returns>
        Task<InventoryItem> AddInventoryItemAsync(InventoryItem item);

        /// <summary>
        /// Updates an existing inventory item
        /// </summary>
        /// <param name="item">The inventory item to update</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateInventoryItemAsync(InventoryItem item);

        /// <summary>
        /// Deletes an inventory item by ID
        /// </summary>
        /// <param name="itemId">The ID of the item to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteInventoryItemAsync(Guid itemId);

        /// <summary>
        /// Verifies if the specified seller owns the specified item
        /// </summary>
        /// <param name="itemId">Item ID to check</param>
        /// <param name="sellerId">Seller ID to verify against</param>
        /// <returns>True if the seller owns the item</returns>
        Task<bool> SellerOwnsItemAsync(Guid itemId, Guid sellerId);

        /// <summary>
        /// Reserves inventory items for an order
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="userId">The user ID making the reservation</param>
        /// <param name="items">The items to reserve with quantities</param>
        /// <param name="expirationTime">When the reservation expires</param>
        /// <returns>Result of the reservation attempt with any unavailable items</returns>
        Task<(bool Success, IEnumerable<UnavailableItem> UnavailableItems)> ReserveInventoryAsync(
            Guid orderId,
            Guid userId,
            IEnumerable<(Guid ItemId, int Quantity)> items,
            DateTime expirationTime);

        /// <summary>
        /// Confirms a reservation, permanently removing items from inventory
        /// </summary>
        /// <param name="orderId">The order ID associated with the reservation</param>
        /// <returns>True if confirmation was successful</returns>
        Task<bool> ConfirmReservationAsync(Guid orderId);

        /// <summary>
        /// Releases a reservation, returning items to available inventory
        /// </summary>
        /// <param name="orderId">The order ID associated with the reservation</param>
        /// <returns>True if release was successful</returns>
        Task<bool> ReleaseReservationAsync(Guid orderId);

        /// <summary>
        /// Cleans up expired reservations, returning items to available inventory
        /// </summary>
        /// <returns>Number of expired reservations cleaned up</returns>
        Task<int> CleanupExpiredReservationsAsync();
    }
} 