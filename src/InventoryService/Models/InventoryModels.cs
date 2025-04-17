using System;
using System.Collections.Generic;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Models
{
    /// <summary>
    /// Represents an inventory item in the system
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the item
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the price of the item
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the available quantity of the item
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Gets or sets the condition of the item
        /// </summary>
        public ItemCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the URL to the item's image
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the ID of the seller who owns this item
        /// </summary>
        public Guid SellerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the seller (populated from join)
        /// </summary>
        public string SellerName { get; set; }

        /// <summary>
        /// Gets or sets the date when the item was added to inventory
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the date when the item was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Represents a paged result of inventory items
    /// </summary>
    public class InventoryPagedResult
    {
        /// <summary>
        /// Gets or sets the list of inventory items
        /// </summary>
        public List<InventoryItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the total number of items matching the search criteria
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages
        /// </summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents an item request in an order
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>
        /// Gets or sets the ID of the requested item
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the requested item
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Represents an item that is unavailable or has insufficient quantity
    /// </summary>
    public class UnavailableItem
    {
        /// <summary>
        /// Gets or sets the ID of the unavailable item
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the requested quantity
        /// </summary>
        public int RequestedQuantity { get; set; }

        /// <summary>
        /// Gets or sets the available quantity
        /// </summary>
        public int AvailableQuantity { get; set; }
    }

    /// <summary>
    /// Represents the result of an inventory operation
    /// </summary>
    public class InventoryOperationResult
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the message associated with the operation result
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the ID of the affected item (if applicable)
        /// </summary>
        public Guid? ItemId { get; }

        private InventoryOperationResult(bool success, string message, Guid? itemId = null)
        {
            Success = success;
            Message = message;
            ItemId = itemId;
        }

        /// <summary>
        /// Creates a successful operation result
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="itemId">Optional item ID</param>
        /// <returns>A successful operation result</returns>
        public static InventoryOperationResult Success(string message, Guid? itemId = null)
        {
            return new InventoryOperationResult(true, message, itemId);
        }

        /// <summary>
        /// Creates a failed operation result
        /// </summary>
        /// <param name="message">Failure message</param>
        /// <returns>A failed operation result</returns>
        public static InventoryOperationResult Failure(string message)
        {
            return new InventoryOperationResult(false, message);
        }
    }

    /// <summary>
    /// Represents the result of an inventory reservation operation
    /// </summary>
    public class InventoryReservationResult
    {
        /// <summary>
        /// Gets a value indicating whether the reservation was successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the message associated with the reservation result
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the list of unavailable items (if applicable)
        /// </summary>
        public List<UnavailableItem> UnavailableItems { get; }

        private InventoryReservationResult(bool success, string message, List<UnavailableItem> unavailableItems = null)
        {
            Success = success;
            Message = message;
            UnavailableItems = unavailableItems ?? new List<UnavailableItem>();
        }

        /// <summary>
        /// Creates a successful reservation result
        /// </summary>
        /// <param name="message">Success message</param>
        /// <returns>A successful reservation result</returns>
        public static InventoryReservationResult Success(string message)
        {
            return new InventoryReservationResult(true, message);
        }

        /// <summary>
        /// Creates a failed reservation result
        /// </summary>
        /// <param name="message">Failure message</param>
        /// <param name="unavailableItems">List of unavailable items</param>
        /// <returns>A failed reservation result</returns>
        public static InventoryReservationResult Failure(string message, List<UnavailableItem> unavailableItems = null)
        {
            return new InventoryReservationResult(false, message, unavailableItems);
        }
    }
} 