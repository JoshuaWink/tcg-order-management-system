using System.Threading.Tasks;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// Interface for publishing order-related events
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish an event when a new order is created
        /// </summary>
        /// <param name="eventData">The order created event data</param>
        Task PublishOrderCreatedEventAsync(OrderCreatedEvent eventData);
        
        /// <summary>
        /// Publish an event when an order is updated
        /// </summary>
        /// <param name="eventData">The order updated event data</param>
        Task PublishOrderUpdatedEventAsync(OrderUpdatedEvent eventData);
        
        /// <summary>
        /// Publish an event when a payment is processed for an order
        /// </summary>
        /// <param name="eventData">The payment processed event data</param>
        Task PublishPaymentProcessedEventAsync(PaymentProcessedEvent eventData);
        
        /// <summary>
        /// Publish an event when inventory is reserved for an order
        /// </summary>
        /// <param name="eventData">The inventory reserved event data</param>
        Task PublishInventoryReservedEventAsync(InventoryReservedEvent eventData);
        
        /// <summary>
        /// Publish an event when inventory reservation fails for an order
        /// </summary>
        /// <param name="eventData">The inventory reservation failed event data</param>
        Task PublishInventoryReservationFailedEventAsync(InventoryReservationFailedEvent eventData);
        
        /// <summary>
        /// Publish an event when shipping rates are calculated for an order
        /// </summary>
        /// <param name="eventData">The shipping rate calculated event data</param>
        Task PublishShippingRateCalculatedEventAsync(ShippingRateCalculatedEvent eventData);
        
        /// <summary>
        /// Publish an event when an order is canceled
        /// </summary>
        /// <param name="eventData">The order canceled event data</param>
        Task PublishOrderCanceledEventAsync(OrderCancelledEvent eventData);
        
        /// <summary>
        /// Publish an event when an order is completed
        /// </summary>
        /// <param name="eventData">The order completed event data</param>
        Task PublishOrderCompletedEventAsync(OrderCompletedEvent eventData);
    }
} 