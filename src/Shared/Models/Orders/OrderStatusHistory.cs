using System;

namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents a record of an order status change
    /// </summary>
    public class OrderStatusHistory
    {
        /// <summary>
        /// Unique identifier for the status history record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Reference to the order
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// The status that was applied
        /// </summary>
        public OrderStatus Status { get; set; }
        
        /// <summary>
        /// When the status change occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Optional comment explaining the status change
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// User who made the status change
        /// </summary>
        public string ChangedBy { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public OrderStatusHistory()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }
} 