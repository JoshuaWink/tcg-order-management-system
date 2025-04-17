using System;
using System.Collections.Generic;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents a generic collectible item in the system
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The SKU (Stock Keeping Unit) of the item
        /// </summary>
        public string Sku { get; set; }
        
        /// <summary>
        /// Name or title of the item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Detailed description of the item
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Category of the collectible item
        /// </summary>
        public ItemCategory Category { get; set; }
        
        /// <summary>
        /// Condition of the item
        /// </summary>
        public ItemCondition Condition { get; set; }
        
        /// <summary>
        /// Current price of the item in USD
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity in inventory
        /// </summary>
        public int QuantityAvailable { get; set; }
        
        /// <summary>
        /// URL to the item's primary image
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Set or collection the item belongs to (if applicable)
        /// </summary>
        public string SetName { get; set; }
        
        /// <summary>
        /// Unique identifier within its set (if applicable)
        /// </summary>
        public string SetIdentifier { get; set; }
        
        /// <summary>
        /// Key attributes specific to this item (varies by category)
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }
        
        /// <summary>
        /// When the item was created in the system
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the item was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Whether the item is currently listed for sale
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Item()
        {
            Id = Guid.NewGuid();
            Attributes = new Dictionary<string, string>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
} 