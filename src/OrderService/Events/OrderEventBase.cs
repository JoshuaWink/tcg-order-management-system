using System;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// Base class for all order-related events
    /// </summary>
    public abstract class OrderEventBase
    {
        /// <summary>
        /// Gets the order ID that this event is associated with
        /// </summary>
        public Guid OrderId { get; }
        
        /// <summary>
        /// Gets the timestamp when the event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEventBase"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        protected OrderEventBase(Guid orderId)
        {
            OrderId = orderId;
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
} 