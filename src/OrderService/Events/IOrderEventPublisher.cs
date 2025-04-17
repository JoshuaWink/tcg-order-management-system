using System;
using System.Threading.Tasks;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// Interface for publishing order-related events
    /// </summary>
    public interface IOrderEventPublisher
    {
        /// <summary>
        /// Publish an order created event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderCreatedEventAsync(OrderCreatedEvent @event);
        
        /// <summary>
        /// Publish an order status changed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderStatusChangedEventAsync(OrderStatusChangedEvent @event);
        
        /// <summary>
        /// Publish an order payment processed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderPaymentProcessedEventAsync(OrderPaymentProcessedEvent @event);
        
        /// <summary>
        /// Publish an order shipping updated event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderShippingUpdatedEventAsync(OrderShippingUpdatedEvent @event);
        
        /// <summary>
        /// Publish an order shipped event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderShippedEventAsync(OrderShippedEvent @event);
        
        /// <summary>
        /// Publish an order cancelled event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderCancelledEventAsync(OrderCancelledEvent @event);
        
        /// <summary>
        /// Publish an order item added event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderItemAddedEventAsync(OrderItemAddedEvent @event);
        
        /// <summary>
        /// Publish an order item removed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderItemRemovedEventAsync(OrderItemRemovedEvent @event);
        
        /// <summary>
        /// Publish an order item quantity updated event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderItemQuantityUpdatedEventAsync(OrderItemQuantityUpdatedEvent @event);
        
        /// <summary>
        /// Publish an order return processed event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderReturnProcessedEventAsync(OrderReturnProcessedEvent @event);
        
        /// <summary>
        /// Publish an order delivered event
        /// </summary>
        /// <param name="event">The event to publish</param>
        Task PublishOrderDeliveredEventAsync(OrderDeliveredEvent @event);
        
        /// <summary>
        /// Publish a generic order event
        /// </summary>
        /// <typeparam name="T">Type of order event</typeparam>
        /// <param name="event">The event to publish</param>
        Task PublishEventAsync<T>(T @event) where T : OrderEvent;
    }
} 