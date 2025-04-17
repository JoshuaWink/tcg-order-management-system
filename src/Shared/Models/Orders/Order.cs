using System;
using System.Collections.Generic;

namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents a customer order in the system
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier for the order
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer who placed the order
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Date and time when the order was created
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Current status of the order
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Shipping information for the order
        /// </summary>
        public ShippingInfo ShippingInfo { get; set; }

        /// <summary>
        /// Payment information for the order
        /// </summary>
        public PaymentInfo PaymentInfo { get; set; }

        /// <summary>
        /// Collection of items in the order
        /// </summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Subtotal before tax and shipping
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// Shipping cost
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Total discount applied to the order
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Final total amount for the order
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Notes associated with the order
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Tracking the history of status changes
        /// </summary>
        public List<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        /// <summary>
        /// Date and time when the order was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Calculate the current order total
        /// </summary>
        public void CalculateTotal()
        {
            Subtotal = 0;
            foreach (var item in Items)
            {
                Subtotal += item.Price * item.Quantity;
            }
            
            Total = Subtotal + Tax + ShippingCost - Discount;
        }

        /// <summary>
        /// Add a status change to the history
        /// </summary>
        public void AddStatusHistory(OrderStatus newStatus, string comment)
        {
            StatusHistory.Add(new OrderStatusHistory
            {
                Status = newStatus,
                Timestamp = DateTime.UtcNow,
                Comment = comment
            });
            
            Status = newStatus;
            LastUpdated = DateTime.UtcNow;
        }
    }
} 