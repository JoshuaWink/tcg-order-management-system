using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCGOrderManagement.Api.Models;
using TCGOrderManagement.InventoryService.Services;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.Api.Controllers
{
    /// <summary>
    /// Controller for inventory operations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;
        
        /// <summary>
        /// Initializes a new instance of the InventoryController
        /// </summary>
        /// <param name="inventoryService">Inventory service</param>
        /// <param name="logger">Logger</param>
        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets available inventory items
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="searchTerm">Optional search term</param>
        /// <returns>List of inventory items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InventoryListResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInventory(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _inventoryService.GetInventoryAsync(page, pageSize, category, searchTerm);
                
                return Ok(ApiResponse<InventoryListResult>.SuccessResponse(result, 
                    $"Retrieved {result.Items?.Count ?? 0} of {result.TotalCount} items"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error retrieving inventory", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Gets inventory item details by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>Item details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            try
            {
                var result = await _inventoryService.GetItemDetailsAsync(id);
                
                if (!result.Success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<InventoryItemResult>.SuccessResponse(result, "Item details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item {ItemId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error retrieving item", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Adds a new inventory item
        /// </summary>
        /// <param name="request">Item request</param>
        /// <returns>Item creation result</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItemRequest request)
        {
            try
            {
                // Set seller ID from authenticated user if not provided
                if (request.SellerId == Guid.Empty)
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
                    {
                        request.SellerId = id;
                    }
                }
                
                var result = await _inventoryService.AddInventoryItemAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                _logger.LogInformation("Item {ItemId} added successfully", result.ItemId);
                
                return CreatedAtAction(nameof(GetItemById), new { id = result.ItemId }, 
                    ApiResponse<InventoryItemResult>.SuccessResponse(result, "Item added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory item");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error adding inventory item", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Updates an existing inventory item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="request">Update request</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemRequest request)
        {
            try
            {
                // Check if user is authorized to update this item
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && !string.IsNullOrEmpty(userIdClaim) && 
                    Guid.TryParse(userIdClaim, out var userId))
                {
                    var itemDetails = await _inventoryService.GetItemDetailsAsync(id);
                    if (itemDetails.Success && itemDetails.SellerId != userId)
                    {
                        return Forbid();
                    }
                }
                
                var result = await _inventoryService.UpdateInventoryItemAsync(id, request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                    }
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<InventoryItemResult>.SuccessResponse(result, "Item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item {ItemId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error updating inventory item", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Deletes an inventory item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInventoryItem(Guid id)
        {
            try
            {
                // Check if user is authorized to delete this item
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && !string.IsNullOrEmpty(userIdClaim) && 
                    Guid.TryParse(userIdClaim, out var userId))
                {
                    var itemDetails = await _inventoryService.GetItemDetailsAsync(id);
                    if (itemDetails.Success && itemDetails.SellerId != userId)
                    {
                        return Forbid();
                    }
                }
                
                var result = await _inventoryService.DeleteInventoryItemAsync(id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                    }
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Item deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item {ItemId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error deleting inventory item", new List<string> { ex.Message }));
            }
        }
    }
    
    /// <summary>
    /// Request model for updating an inventory item
    /// </summary>
    public class UpdateInventoryItemRequest
    {
        /// <summary>
        /// Updated item name
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Updated item description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Updated item price
        /// </summary>
        public decimal? Price { get; set; }
        
        /// <summary>
        /// Updated item quantity
        /// </summary>
        public int? Quantity { get; set; }
        
        /// <summary>
        /// Updated item condition
        /// </summary>
        public ItemCondition? Condition { get; set; }
        
        /// <summary>
        /// Updated item category
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// Updated item image URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
} 