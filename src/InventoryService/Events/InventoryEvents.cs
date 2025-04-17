using System;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Events
{
    /// <summary>
    /// Base class for all inventory events
    /// </summary>
    public abstract class InventoryEventBase
    {
        /// <summary>
        /// Unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// SKU of the item
        /// </summary>
        public string Sku { get; set; }
        
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Event raised when a new item is created
    /// </summary>
    public class ItemCreatedEvent : InventoryEventBase
    {
        /// <summary>
        /// Category of the item
        /// </summary>
        public ItemCategory Category { get; set; }
        
        /// <summary>
        /// Price of the item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Initial quantity available
        /// </summary>
        public int QuantityAvailable { get; set; }
    }
    
    /// <summary>
    /// Event raised when an item is updated
    /// </summary>
    public class ItemUpdatedEvent : InventoryEventBase
    {
        /// <summary>
        /// Category of the item
        /// </summary>
        public ItemCategory Category { get; set; }
        
        /// <summary>
        /// Updated price of the item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Updated quantity available
        /// </summary>
        public int QuantityAvailable { get; set; }
    }
    
    /// <summary>
    /// Event raised when an item is deleted
    /// </summary>
    public class ItemDeletedEvent : InventoryEventBase
    {
    }
    
    /// <summary>
    /// Event raised when an item's inventory quantity changes
    /// </summary>
    public class InventoryChangedEvent : InventoryEventBase
    {
        /// <summary>
        /// The quantity change amount (positive for increase, negative for decrease)
        /// </summary>
        public int QuantityChange { get; set; }
        
        /// <summary>
        /// The new total quantity after the change
        /// </summary>
        public int NewQuantity { get; set; }
    }
    
    /// <summary>
    /// Event raised when an item's inventory becomes low
    /// </summary>
    public class LowInventoryEvent : InventoryEventBase
    {
        /// <summary>
        /// The current inventory quantity
        /// </summary>
        public int Quantity { get; set; }
    }
} 