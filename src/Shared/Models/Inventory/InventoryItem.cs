using System;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.Shared.Models.Inventory
{
    /// <summary>
    /// Represents an item in inventory with stock information
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// Unique identifier for the inventory record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Reference to the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// The item details
        /// </summary>
        public Item Item { get; set; }
        
        /// <summary>
        /// Current stock quantity
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Location or bin where the item is stored
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Minimum threshold for reordering
        /// </summary>
        public int ReorderThreshold { get; set; }
        
        /// <summary>
        /// Whether the item is available for sale
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Date when the inventory record was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Username of who last updated the inventory
        /// </summary>
        public string LastUpdatedBy { get; set; }
    }
} 