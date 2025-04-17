using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Services
{
    /// <summary>
    /// Interface for item service providing business logic for inventory operations
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Get an item by its unique identifier
        /// </summary>
        /// <param name="id">The item's unique identifier</param>
        /// <returns>The item if found</returns>
        Task<Item> GetItemByIdAsync(Guid id);
        
        /// <summary>
        /// Get an item by its SKU
        /// </summary>
        /// <param name="sku">The item's SKU</param>
        /// <returns>The item if found</returns>
        Task<Item> GetItemBySkuAsync(string sku);
        
        /// <summary>
        /// Search for items based on various criteria
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="condition">Optional condition filter</param>
        /// <param name="minPrice">Optional minimum price filter</param>
        /// <param name="maxPrice">Optional maximum price filter</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Collection of items matching the criteria</returns>
        Task<IEnumerable<Item>> SearchItemsAsync(
            string searchTerm,
            ItemCategory? category = null,
            ItemCondition? condition = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Add a new item to inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The added item with updated information</returns>
        Task<Item> AddItemAsync(Item item);
        
        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="item">The item with updated information</param>
        /// <returns>The updated item</returns>
        Task<Item> UpdateItemAsync(Item item);
        
        /// <summary>
        /// Delete an item from inventory
        /// </summary>
        /// <param name="id">The item's unique identifier</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteItemAsync(Guid id);
        
        /// <summary>
        /// Update the inventory quantity for an item
        /// </summary>
        /// <param name="id">The item's unique identifier</param>
        /// <param name="quantityChange">The quantity change (positive for increase, negative for decrease)</param>
        /// <returns>The new quantity after the update</returns>
        Task<int> UpdateInventoryQuantityAsync(Guid id, int quantityChange);
        
        /// <summary>
        /// Get items by their condition
        /// </summary>
        /// <param name="condition">The condition to filter by</param>
        /// <returns>Collection of items with the specified condition</returns>
        Task<IEnumerable<Item>> GetItemsByConditionAsync(ItemCondition condition);
        
        /// <summary>
        /// Get items by their category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>Collection of items in the specified category</returns>
        Task<IEnumerable<Item>> GetItemsByCategoryAsync(ItemCategory category);
        
        /// <summary>
        /// Get items within a price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>Collection of items within the price range</returns>
        Task<IEnumerable<Item>> GetItemsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        
        /// <summary>
        /// Get all active items
        /// </summary>
        /// <returns>Collection of active items</returns>
        Task<IEnumerable<Item>> GetActiveItemsAsync();
    }
} 