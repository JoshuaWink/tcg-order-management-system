using System;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents a single item in an order
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Unique identifier for the order item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Reference to the related order
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Reference to the inventory item
        /// </summary>
        public Guid InventoryItemId { get; set; }
        
        /// <summary>
        /// The basic item information
        /// </summary>
        public Item Item { get; set; }
        
        /// <summary>
        /// Condition of the item when ordered
        /// </summary>
        public string Condition { get; set; }
        
        /// <summary>
        /// Price of the item at the time of order
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Quantity of this item ordered
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Current status of this order item
        /// </summary>
        public OrderItemStatus Status { get; set; }
        
        /// <summary>
        /// ID of the vendor providing this item
        /// </summary>
        public Guid VendorId { get; set; }
        
        /// <summary>
        /// Name of the vendor
        /// </summary>
        public string VendorName { get; set; }
        
        /// <summary>
        /// Notes specific to this order item
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Line total for this item (Price * Quantity)
        /// </summary>
        public decimal LineTotal => Price * Quantity;
    }
} 