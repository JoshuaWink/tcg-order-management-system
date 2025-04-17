using System.Threading.Tasks;
using TCGOrderManagement.OrderService.Events;

namespace TCGOrderManagement.OrderService.Interfaces
{
    /// <summary>
    /// Interface for publishing order-related events to the message broker
    /// </summary>
    public interface IOrderEventPublisher
    {
        /// <summary>
        /// Publishes an event when an order is created
        /// </summary>
        /// <param name="orderCreatedEvent">The order created event</param>
        Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderCreatedEvent);

        /// <summary>
        /// Publishes an event when an order's status is changed
        /// </summary>
        /// <param name="orderStatusChangedEvent">The order status changed event</param>
        Task PublishOrderStatusChangedEventAsync(OrderStatusChangedEvent orderStatusChangedEvent);

        /// <summary>
        /// Publishes an event when an order's payment is processed
        /// </summary>
        /// <param name="orderPaymentProcessedEvent">The order payment processed event</param>
        Task PublishOrderPaymentProcessedEventAsync(OrderPaymentProcessedEvent orderPaymentProcessedEvent);

        /// <summary>
        /// Publishes an event when an order's shipping information is updated
        /// </summary>
        /// <param name="orderShippingUpdatedEvent">The order shipping updated event</param>
        Task PublishOrderShippingUpdatedEventAsync(OrderShippingUpdatedEvent orderShippingUpdatedEvent);

        /// <summary>
        /// Publishes an event when an order is shipped
        /// </summary>
        /// <param name="orderShippedEvent">The order shipped event</param>
        Task PublishOrderShippedEventAsync(OrderShippedEvent orderShippedEvent);

        /// <summary>
        /// Publishes an event when an order is cancelled
        /// </summary>
        /// <param name="orderCancelledEvent">The order cancelled event</param>
        Task PublishOrderCancelledEventAsync(OrderCancelledEvent orderCancelledEvent);

        /// <summary>
        /// Publishes an event when an item is added to an order
        /// </summary>
        /// <param name="orderItemAddedEvent">The order item added event</param>
        Task PublishOrderItemAddedEventAsync(OrderItemAddedEvent orderItemAddedEvent);

        /// <summary>
        /// Publishes an event when an item is removed from an order
        /// </summary>
        /// <param name="orderItemRemovedEvent">The order item removed event</param>
        Task PublishOrderItemRemovedEventAsync(OrderItemRemovedEvent orderItemRemovedEvent);

        /// <summary>
        /// Publishes an event when an order item's quantity is updated
        /// </summary>
        /// <param name="orderItemQuantityUpdatedEvent">The order item quantity updated event</param>
        Task PublishOrderItemQuantityUpdatedEventAsync(OrderItemQuantityUpdatedEvent orderItemQuantityUpdatedEvent);

        /// <summary>
        /// Publishes an event when an order return is processed
        /// </summary>
        /// <param name="orderReturnProcessedEvent">The order return processed event</param>
        Task PublishOrderReturnProcessedEventAsync(OrderReturnProcessedEvent orderReturnProcessedEvent);

        /// <summary>
        /// Publishes an event when an order is delivered
        /// </summary>
        /// <param name="orderDeliveredEvent">The order delivered event</param>
        Task PublishOrderDeliveredEventAsync(OrderDeliveredEvent orderDeliveredEvent);
    }
} 