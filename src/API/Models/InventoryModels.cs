using System;
using System.Collections.Generic;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.Api.Models
{
    /// <summary>
    /// Request model for creating a new inventory item
    /// </summary>
    public class InventoryItemRequest
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Price of the item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Condition of the item
        /// </summary>
        public ItemCondition Condition { get; set; }
        
        /// <summary>
        /// Category of the item
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// URL to the item image
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// ID of the seller (if not provided, taken from authenticated user)
        /// </summary>
        public Guid SellerId { get; set; }
        
        /// <summary>
        /// Optional set of attributes specific to the item type (card name, set, rarity, etc.)
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Result of inventory list operation
    /// </summary>
    public class InventoryListResult
    {
        /// <summary>
        /// List of inventory items
        /// </summary>
        public List<InventoryItemSummary> Items { get; set; } = new List<InventoryItemSummary>();
        
        /// <summary>
        /// Total count of items (for pagination)
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Current page
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
    
    /// <summary>
    /// Summary information for an inventory item
    /// </summary>
    public class InventoryItemSummary
    {
        /// <summary>
        /// ID of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Short description (truncated) 
        /// </summary>
        public string ShortDescription { get; set; }
        
        /// <summary>
        /// Price of the item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity
        /// </summary>
        public int AvailableQuantity { get; set; }
        
        /// <summary>
        /// Item condition
        /// </summary>
        public ItemCondition Condition { get; set; }
        
        /// <summary>
        /// Category of the item
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Thumbnail image URL
        /// </summary>
        public string ThumbnailUrl { get; set; }
        
        /// <summary>
        /// ID of the seller
        /// </summary>
        public Guid SellerId { get; set; }
        
        /// <summary>
        /// Name of the seller
        /// </summary>
        public string SellerName { get; set; }
        
        /// <summary>
        /// Seller rating
        /// </summary>
        public decimal SellerRating { get; set; }
        
        /// <summary>
        /// Date when the item was listed
        /// </summary>
        public DateTime ListedDate { get; set; }
    }
    
    /// <summary>
    /// Detailed information for an inventory item
    /// </summary>
    public class InventoryItemResult
    {
        /// <summary>
        /// Whether operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// ID of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Full description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Price of the item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity
        /// </summary>
        public int AvailableQuantity { get; set; }
        
        /// <summary>
        /// Item condition
        /// </summary>
        public ItemCondition Condition { get; set; }
        
        /// <summary>
        /// Category of the item
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Full-size image URL
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// ID of the seller
        /// </summary>
        public Guid SellerId { get; set; }
        
        /// <summary>
        /// Name of the seller
        /// </summary>
        public string SellerName { get; set; }
        
        /// <summary>
        /// Seller rating
        /// </summary>
        public decimal SellerRating { get; set; }
        
        /// <summary>
        /// Date when the item was listed
        /// </summary>
        public DateTime ListedDate { get; set; }
        
        /// <summary>
        /// Date when the item was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Set of attributes specific to the item type (card name, set, rarity, etc.)
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Related items by the same seller
        /// </summary>
        public List<InventoryItemSummary> RelatedItems { get; set; } = new List<InventoryItemSummary>();
    }
} 