namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents the possible statuses of an order
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been created but not yet processed
        /// </summary>
        Created = 0,
        
        /// <summary>
        /// Payment has been received and order is awaiting processing
        /// </summary>
        Paid = 10,
        
        /// <summary>
        /// Order is being processed (items pulled from inventory)
        /// </summary>
        Processing = 20,
        
        /// <summary>
        /// Order is awaiting inventory (some items are on backorder)
        /// </summary>
        AwaitingInventory = 30,
        
        /// <summary>
        /// Items have been packaged and are ready to ship
        /// </summary>
        ReadyToShip = 40,
        
        /// <summary>
        /// Order has been shipped
        /// </summary>
        Shipped = 50,
        
        /// <summary>
        /// Order has been delivered to the customer
        /// </summary>
        Delivered = 60,
        
        /// <summary>
        /// Order is completed (delivered and no returns/issues)
        /// </summary>
        Completed = 70,
        
        /// <summary>
        /// Order has a pending return
        /// </summary>
        ReturnPending = 80,
        
        /// <summary>
        /// Return has been received and processed
        /// </summary>
        Returned = 90,
        
        /// <summary>
        /// Order has been cancelled
        /// </summary>
        Cancelled = 100,
        
        /// <summary>
        /// Order has been refunded
        /// </summary>
        Refunded = 110
    }
} 