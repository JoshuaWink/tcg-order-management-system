using System;
using System.Collections.Generic;

namespace TCGOrderManagement.Shared.Models
{
    /// <summary>
    /// Represents a base inventory item in the system
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Item SKU (Stock Keeping Unit)
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Item description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of the item (e.g., "Trading Cards", "Accessories", etc.)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Current price of the item
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Weight of the item in grams
        /// </summary>
        public decimal WeightInGrams { get; set; }

        /// <summary>
        /// Date when the item was added to inventory
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Date when the item was last modified
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indicates if the item is currently active and available for sale
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Additional attributes stored as key-value pairs
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }
    }
} 