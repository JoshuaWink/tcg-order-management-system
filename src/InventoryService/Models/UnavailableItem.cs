using System;

namespace TCGOrderManagement.InventoryService.Models
{
    /// <summary>
    /// Represents an item that could not be reserved due to insufficient quantity
    /// </summary>
    public class UnavailableItem
    {
        /// <summary>
        /// Gets the ID of the inventory item
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// Gets the name of the item
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// Gets the quantity that was requested
        /// </summary>
        public int RequestedQuantity { get; }

        /// <summary>
        /// Gets the quantity that was actually available
        /// </summary>
        public int AvailableQuantity { get; }

        /// <summary>
        /// Initializes a new instance of the UnavailableItem class
        /// </summary>
        /// <param name="itemId">The ID of the item</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="requestedQuantity">The quantity that was requested</param>
        /// <param name="availableQuantity">The quantity that was available</param>
        public UnavailableItem(Guid itemId, string itemName, int requestedQuantity, int availableQuantity)
        {
            ItemId = itemId;
            ItemName = itemName;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
} 