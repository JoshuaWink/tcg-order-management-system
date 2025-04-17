using System.Threading.Tasks;

namespace TCGOrderManagement.InventoryService.Events
{
    /// <summary>
    /// Interface for publishing inventory-related events
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish an event when a new item is created
        /// </summary>
        /// <param name="eventData">The item created event data</param>
        Task PublishItemCreatedEventAsync(ItemCreatedEvent eventData);
        
        /// <summary>
        /// Publish an event when an item is updated
        /// </summary>
        /// <param name="eventData">The item updated event data</param>
        Task PublishItemUpdatedEventAsync(ItemUpdatedEvent eventData);
        
        /// <summary>
        /// Publish an event when an item is deleted
        /// </summary>
        /// <param name="eventData">The item deleted event data</param>
        Task PublishItemDeletedEventAsync(ItemDeletedEvent eventData);
        
        /// <summary>
        /// Publish an event when an item's inventory quantity changes
        /// </summary>
        /// <param name="eventData">The inventory changed event data</param>
        Task PublishInventoryChangedEventAsync(InventoryChangedEvent eventData);
        
        /// <summary>
        /// Publish an event when an item's inventory becomes low
        /// </summary>
        /// <param name="eventData">The low inventory event data</param>
        Task PublishLowInventoryEventAsync(LowInventoryEvent eventData);
    }
} 