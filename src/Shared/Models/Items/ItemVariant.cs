using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents a specific variant of an item with its own condition and pricing
    /// </summary>
    public class ItemVariant
    {
        /// <summary>
        /// Unique identifier for the item variant
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the parent item
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Navigation property to the parent item
        /// </summary>
        public virtual Item Item { get; set; }

        /// <summary>
        /// Specific variation identifier (if applicable)
        /// </summary>
        public string VariantCode { get; set; }

        /// <summary>
        /// Physical condition of the item
        /// </summary>
        public ItemCondition Condition { get; set; }

        /// <summary>
        /// Professional grading information (if applicable)
        /// </summary>
        public string GradingInfo { get; set; }

        /// <summary>
        /// Current market price for this variant
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// Listing price set by the seller
        /// </summary>
        public decimal ListingPrice { get; set; }

        /// <summary>
        /// Quantity available in inventory
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Quantity reserved for pending orders
        /// </summary>
        public int QuantityReserved { get; set; }

        /// <summary>
        /// Indicates if this variant is currently in stock
        /// </summary>
        public bool InStock => QuantityAvailable > QuantityReserved;

        /// <summary>
        /// Seller who listed this item variant
        /// </summary>
        public Guid SellerId { get; set; }

        /// <summary>
        /// Date when this variant was added to the system
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Date when this variant was last modified
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indicates whether this variant is currently active in the marketplace
        /// </summary>
        public bool IsActive { get; set; }
    }
} 