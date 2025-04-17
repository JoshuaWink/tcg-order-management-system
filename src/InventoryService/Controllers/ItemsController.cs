using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.InventoryService.Exceptions;
using TCGOrderManagement.InventoryService.Services;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get an item by its unique identifier
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The requested item</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Item>> GetItemByIdAsync(Guid id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                return Ok(item);
            }
            catch (ItemNotFoundException ex)
            {
                _logger.LogWarning(ex, "Item not found: {ItemId}", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item: {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get an item by its SKU
        /// </summary>
        /// <param name="sku">Item SKU</param>
        /// <returns>The requested item</returns>
        [HttpGet("sku/{sku}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Item>> GetItemBySkuAsync(string sku)
        {
            try
            {
                var item = await _itemService.GetItemBySkuAsync(sku);
                return Ok(item);
            }
            catch (ItemNotFoundException ex)
            {
                _logger.LogWarning(ex, "Item not found: {Sku}", sku);
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument: {Sku}", sku);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item: {Sku}", sku);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Search for items based on various criteria
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="condition">Optional condition filter</param>
        /// <param name="minPrice">Optional minimum price</param>
        /// <param name="maxPrice">Optional maximum price</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Collection of items matching search criteria</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Item>>> SearchItemsAsync(
            [FromQuery] string searchTerm,
            [FromQuery] ItemCategory? category = null,
            [FromQuery] ItemCondition? condition = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var items = await _itemService.SearchItemsAsync(searchTerm, category, condition, minPrice, maxPrice, page, pageSize);
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search parameters: {SearchTerm}", searchTerm);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching items: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        /// <param name="category">Item category</param>
        /// <returns>Collection of items in the specified category</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsByCategoryAsync(ItemCategory category)
        {
            try
            {
                var items = await _itemService.GetItemsByCategoryAsync(category);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items by category: {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get items by condition
        /// </summary>
        /// <param name="condition">Item condition</param>
        /// <returns>Collection of items in the specified condition</returns>
        [HttpGet("condition/{condition}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsByConditionAsync(ItemCondition condition)
        {
            try
            {
                var items = await _itemService.GetItemsByConditionAsync(condition);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items by condition: {Condition}", condition);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get items by price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>Collection of items within the price range</returns>
        [HttpGet("pricerange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsByPriceRangeAsync(
            [FromQuery] decimal minPrice = 0,
            [FromQuery] decimal maxPrice = decimal.MaxValue)
        {
            try
            {
                var items = await _itemService.GetItemsByPriceRangeAsync(minPrice, maxPrice);
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get all active items
        /// </summary>
        /// <returns>Collection of active items</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Item>>> GetActiveItemsAsync()
        {
            try
            {
                var items = await _itemService.GetActiveItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active items");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Add a new item
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>The newly created item</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Item>> AddItemAsync([FromBody] Item item)
        {
            try
            {
                var addedItem = await _itemService.AddItemAsync(item);
                return CreatedAtAction(nameof(GetItemByIdAsync), new { id = addedItem.Id }, addedItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid item data: {ItemName}", item?.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (DuplicateItemException ex)
            {
                _logger.LogWarning(ex, "Duplicate item: {ItemSku}", item?.Sku);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item: {ItemName}", item?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="item">Updated item data</param>
        /// <returns>The updated item</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Item>> UpdateItemAsync(Guid id, [FromBody] Item item)
        {
            try
            {
                if (id != item.Id)
                {
                    return BadRequest(new { Message = "ID in URL does not match ID in request body." });
                }

                var updatedItem = await _itemService.UpdateItemAsync(item);
                return Ok(updatedItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid item data: {ItemId}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (ItemNotFoundException ex)
            {
                _logger.LogWarning(ex, "Item not found: {ItemId}", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (DuplicateItemException ex)
            {
                _logger.LogWarning(ex, "Duplicate item: {ItemSku}", item?.Sku);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="id">ID of the item to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItemAsync(Guid id)
        {
            try
            {
                var result = await _itemService.DeleteItemAsync(id);
                return NoContent();
            }
            catch (ItemNotFoundException ex)
            {
                _logger.LogWarning(ex, "Item not found: {ItemId}", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Update inventory quantity
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="quantityChange">Quantity change (positive for increase, negative for decrease)</param>
        /// <returns>The new quantity</returns>
        [HttpPatch("{id:guid}/quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateInventoryQuantityAsync(Guid id, [FromBody] int quantityChange)
        {
            try
            {
                var newQuantity = await _itemService.UpdateInventoryQuantityAsync(id, quantityChange);
                return Ok(new { NewQuantity = newQuantity });
            }
            catch (ItemNotFoundException ex)
            {
                _logger.LogWarning(ex, "Item not found: {ItemId}", id);
                return NotFound(new { Message = ex.Message });
            }
            catch (InsufficientInventoryException ex)
            {
                _logger.LogWarning(ex, "Insufficient inventory: {ItemId}, Requested Change: {QuantityChange}", id, quantityChange);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory: {ItemId}, Change: {QuantityChange}", id, quantityChange);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
            }
        }
    }
} 