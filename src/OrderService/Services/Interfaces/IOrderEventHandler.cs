using System.Threading.Tasks;
using TCGOrderManagement.OrderService.Events;

namespace TCGOrderManagement.OrderService.Services.Interfaces
{
    /// <summary>
    /// Interface defining methods for handling order-related events
    /// </summary>
    public interface IOrderEventHandler
    {
        /// <summary>
        /// Handles the payment processed event
        /// </summary>
        /// <param name="paymentEvent">The payment processed event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandlePaymentProcessedAsync(PaymentProcessedEvent paymentEvent);

        /// <summary>
        /// Handles the inventory reserved event
        /// </summary>
        /// <param name="inventoryEvent">The inventory reserved event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandleInventoryReservedAsync(InventoryReservedEvent inventoryEvent);

        /// <summary>
        /// Handles the inventory reservation failed event
        /// </summary>
        /// <param name="inventoryEvent">The inventory reservation failed event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandleInventoryReservationFailedAsync(InventoryReservationFailedEvent inventoryEvent);

        /// <summary>
        /// Handles the shipping rate calculated event
        /// </summary>
        /// <param name="shippingEvent">The shipping rate calculated event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandleShippingRateCalculatedAsync(ShippingRateCalculatedEvent shippingEvent);
    }
} 