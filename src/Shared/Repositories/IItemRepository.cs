using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.Shared.Repositories
{
    /// <summary>
    /// Interface for item repository providing data access operations for collectible items
    /// </summary>
    public interface IItemRepository
    {
        /// <summary>
        /// Get an item by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the item</param>
        /// <returns>The item if found, null otherwise</returns>
        Task<Item> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get an item by its SKU
        /// </summary>
        /// <param name="sku">The SKU of the item</param>
        /// <returns>The item if found, null otherwise</returns>
        Task<Item> GetBySkuAsync(string sku);
        
        /// <summary>
        /// Get all items matching the specified criteria
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="condition">Optional condition filter</param>
        /// <param name="minPrice">Optional minimum price filter</param>
        /// <param name="maxPrice">Optional maximum price filter</param>
        /// <param name="isActive">Optional filter for active/inactive items</param>
        /// <returns>A collection of items matching the criteria</returns>
        Task<IEnumerable<Item>> GetAllAsync(
            ItemCategory? category = null, 
            ItemCondition? condition = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null);
        
        /// <summary>
        /// Search for items by name, description or other searchable fields
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="condition">Optional condition filter</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>A paged collection of items matching the search criteria</returns>
        Task<(IEnumerable<Item> Items, int TotalCount)> SearchAsync(
            string searchTerm,
            ItemCategory? category = null,
            ItemCondition? condition = null,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Add a new item to the repository
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The added item with any generated fields (like ID)</returns>
        Task<Item> AddAsync(Item item);
        
        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        Task<Item> UpdateAsync(Item item);
        
        /// <summary>
        /// Delete an item by its ID
        /// </summary>
        /// <param name="id">The ID of the item to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Update the inventory quantity for an item
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="quantityChange">The quantity change (positive for increase, negative for decrease)</param>
        /// <returns>The updated available quantity</returns>
        Task<int> UpdateInventoryQuantityAsync(Guid id, int quantityChange);
        
        /// <summary>
        /// Check if an item exists by ID
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>True if the item exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid id);
    }
} 