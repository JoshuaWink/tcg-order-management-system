namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents the possible statuses of an order item
    /// </summary>
    public enum OrderItemStatus
    {
        /// <summary>
        /// Item is pending processing
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Item has been allocated from inventory
        /// </summary>
        Allocated = 1,
        
        /// <summary>
        /// Item is out of stock
        /// </summary>
        OutOfStock = 2,
        
        /// <summary>
        /// Item is being prepared for shipment
        /// </summary>
        InPreparation = 3,
        
        /// <summary>
        /// Item has been shipped
        /// </summary>
        Shipped = 4,
        
        /// <summary>
        /// Item has been delivered
        /// </summary>
        Delivered = 5,
        
        /// <summary>
        /// Item has been returned
        /// </summary>
        Returned = 6,
        
        /// <summary>
        /// Item order has been canceled
        /// </summary>
        Canceled = 7,
        
        /// <summary>
        /// Item is on backorder
        /// </summary>
        Backordered = 8
    }
} 