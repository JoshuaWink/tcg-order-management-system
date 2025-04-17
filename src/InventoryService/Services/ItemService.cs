using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.InventoryService.Events;
using TCGOrderManagement.InventoryService.Exceptions;
using TCGOrderManagement.Shared.Models.Items;
using TCGOrderManagement.Shared.Repositories;

namespace TCGOrderManagement.InventoryService.Services
{
    /// <summary>
    /// Service responsible for managing inventory items
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IEventPublisher _eventPublisher;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepository">Item repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public ItemService(IItemRepository itemRepository, IEventPublisher eventPublisher)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }
        
        /// <inheritdoc />
        public async Task<Item> GetItemByIdAsync(Guid id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            
            if (item == null)
            {
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            }
            
            return item;
        }
        
        /// <inheritdoc />
        public async Task<Item> GetItemBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException("SKU cannot be null or empty.", nameof(sku));
            }
            
            var item = await _itemRepository.GetBySkuAsync(sku);
            
            if (item == null)
            {
                throw new ItemNotFoundException($"Item with SKU {sku} not found.");
            }
            
            return item;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Item>> SearchItemsAsync(
            string searchTerm,
            ItemCategory? category = null,
            ItemCondition? condition = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,
            int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty.", nameof(searchTerm));
            }
            
            if (page < 1)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(page));
            }
            
            if (pageSize < 1)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
            
            var result = await _itemRepository.SearchAsync(searchTerm, category, condition, page, pageSize);
            return result.Items;
        }
        
        /// <inheritdoc />
        public async Task<Item> AddItemAsync(Item item)
        {
            ValidateItem(item);
            
            // Check if an item with the same SKU already exists
            var existingItem = await _itemRepository.GetBySkuAsync(item.Sku);
            if (existingItem != null)
            {
                throw new DuplicateItemException($"An item with SKU {item.Sku} already exists.");
            }
            
            var addedItem = await _itemRepository.AddAsync(item);
            
            // Publish event for the new item
            await _eventPublisher.PublishItemCreatedEventAsync(new ItemCreatedEvent
            {
                ItemId = addedItem.Id,
                Sku = addedItem.Sku,
                Name = addedItem.Name,
                Category = addedItem.Category,
                Price = addedItem.Price,
                QuantityAvailable = addedItem.QuantityAvailable,
                Timestamp = DateTime.UtcNow
            });
            
            return addedItem;
        }
        
        /// <inheritdoc />
        public async Task<Item> UpdateItemAsync(Item item)
        {
            ValidateItem(item);
            
            // Check if the item exists
            var existingItem = await _itemRepository.GetByIdAsync(item.Id);
            if (existingItem == null)
            {
                throw new ItemNotFoundException($"Item with ID {item.Id} not found.");
            }
            
            // Check if another item with the same SKU exists (if SKU has changed)
            if (existingItem.Sku != item.Sku)
            {
                var itemWithSku = await _itemRepository.GetBySkuAsync(item.Sku);
                if (itemWithSku != null && itemWithSku.Id != item.Id)
                {
                    throw new DuplicateItemException($"Another item with SKU {item.Sku} already exists.");
                }
            }
            
            var updatedItem = await _itemRepository.UpdateAsync(item);
            
            // Publish event for the updated item
            await _eventPublisher.PublishItemUpdatedEventAsync(new ItemUpdatedEvent
            {
                ItemId = updatedItem.Id,
                Sku = updatedItem.Sku,
                Name = updatedItem.Name,
                Category = updatedItem.Category,
                Price = updatedItem.Price,
                QuantityAvailable = updatedItem.QuantityAvailable,
                Timestamp = DateTime.UtcNow
            });
            
            return updatedItem;
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteItemAsync(Guid id)
        {
            // Check if the item exists
            var existingItem = await _itemRepository.GetByIdAsync(id);
            if (existingItem == null)
            {
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            }
            
            var result = await _itemRepository.DeleteAsync(id);
            
            if (result)
            {
                // Publish event for the deleted item
                await _eventPublisher.PublishItemDeletedEventAsync(new ItemDeletedEvent
                {
                    ItemId = id,
                    Sku = existingItem.Sku,
                    Name = existingItem.Name,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return result;
        }
        
        /// <inheritdoc />
        public async Task<int> UpdateInventoryQuantityAsync(Guid id, int quantityChange)
        {
            // Check if the item exists
            var existingItem = await _itemRepository.GetByIdAsync(id);
            if (existingItem == null)
            {
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            }
            
            // Check if the quantity would go negative
            if (existingItem.QuantityAvailable + quantityChange < 0)
            {
                throw new InsufficientInventoryException(
                    $"Cannot reduce inventory below zero. Current quantity: {existingItem.QuantityAvailable}, Requested change: {quantityChange}");
            }
            
            var newQuantity = await _itemRepository.UpdateInventoryQuantityAsync(id, quantityChange);
            
            // Publish event for the inventory change
            await _eventPublisher.PublishInventoryChangedEventAsync(new InventoryChangedEvent
            {
                ItemId = id,
                Sku = existingItem.Sku,
                Name = existingItem.Name,
                QuantityChange = quantityChange,
                NewQuantity = newQuantity,
                Timestamp = DateTime.UtcNow
            });
            
            // If inventory is low, publish a low inventory event
            if (newQuantity <= 5 && existingItem.QuantityAvailable > 5)
            {
                await _eventPublisher.PublishLowInventoryEventAsync(new LowInventoryEvent
                {
                    ItemId = id,
                    Sku = existingItem.Sku,
                    Name = existingItem.Name,
                    Quantity = newQuantity,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return newQuantity;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetItemsByConditionAsync(ItemCondition condition)
        {
            return await _itemRepository.GetAllAsync(condition: condition);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetItemsByCategoryAsync(ItemCategory category)
        {
            return await _itemRepository.GetAllAsync(category: category);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetItemsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0)
            {
                throw new ArgumentException("Minimum price cannot be negative.", nameof(minPrice));
            }
            
            if (maxPrice < minPrice)
            {
                throw new ArgumentException("Maximum price cannot be less than minimum price.", nameof(maxPrice));
            }
            
            return await _itemRepository.GetAllAsync(minPrice: minPrice, maxPrice: maxPrice);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetActiveItemsAsync()
        {
            return await _itemRepository.GetAllAsync(isActive: true);
        }
        
        #region Helper Methods
        
        private void ValidateItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            
            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                throw new ArgumentException("Item SKU is required.", nameof(item));
            }
            
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Item name is required.", nameof(item));
            }
            
            if (item.Price < 0)
            {
                throw new ArgumentException("Item price cannot be negative.", nameof(item));
            }
            
            if (item.QuantityAvailable < 0)
            {
                throw new ArgumentException("Item quantity cannot be negative.", nameof(item));
            }
            
            // Validate specific item types
            switch (item)
            {
                case TradingCard card:
                    ValidateTradingCard(card);
                    break;
                case SealedProduct product:
                    ValidateSealedProduct(product);
                    break;
            }
        }
        
        private void ValidateTradingCard(TradingCard card)
        {
            if (string.IsNullOrWhiteSpace(card.Game))
            {
                throw new ArgumentException("Trading card game is required.", nameof(card));
            }
            
            if (string.IsNullOrWhiteSpace(card.Set))
            {
                throw new ArgumentException("Trading card set is required.", nameof(card));
            }
        }
        
        private void ValidateSealedProduct(SealedProduct product)
        {
            if (string.IsNullOrWhiteSpace(product.Game))
            {
                throw new ArgumentException("Sealed product game is required.", nameof(product));
            }
            
            if (string.IsNullOrWhiteSpace(product.ProductType))
            {
                throw new ArgumentException("Sealed product type is required.", nameof(product));
            }
            
            if (product.ItemCount <= 0)
            {
                throw new ArgumentException("Sealed product item count must be positive.", nameof(product));
            }
        }
        
        #endregion
    }
} 