using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCGOrderManagement.Api.Models;
using TCGOrderManagement.OrderService.Services;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.Api.Controllers
{
    /// <summary>
    /// Controller for order operations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;
        
        /// <summary>
        /// Initializes a new instance of the OrdersController
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="logger">Logger</param>
        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        
        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="request">Order request</param>
        /// <returns>Order creation result</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderResult>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                // Set customer ID from authenticated user if not provided
                if (request.CustomerId == Guid.Empty)
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
                    {
                        request.CustomerId = id;
                    }
                }
                
                var result = await _orderService.CreateOrderAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                _logger.LogInformation("Order {OrderId} created successfully", result.OrderId);
                
                return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId }, 
                    ApiResponse<OrderResult>.SuccessResponse(result, "Order created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error creating order", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDetailResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var result = await _orderService.GetOrderDetailsAsync(id);
                
                if (!result.Success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<OrderDetailResult>.SuccessResponse(result, "Order details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error retrieving order", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Gets orders for the authenticated customer
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of orders</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OrdersResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get customer ID from authenticated user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var customerId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
                }
                
                var result = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);
                
                return Ok(ApiResponse<OrdersResult>.SuccessResponse(result, 
                    $"Retrieved {result.Orders?.Count ?? 0} of {result.TotalCount} orders"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer orders");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error retrieving orders", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Cancels an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="request">Cancellation request</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OrderResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id, request.Reason);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                    }
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<OrderResult>.SuccessResponse(result, "Order cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error cancelling order", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Updates the status of an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Status update result</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(typeof(ApiResponse<OrderResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, request.Notes);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse(result.Message));
                    }
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }
                
                return Ok(ApiResponse<OrderResult>.SuccessResponse(result, "Order status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Error updating order status", new List<string> { ex.Message }));
            }
        }
    }
    
    /// <summary>
    /// Request model for cancelling an order
    /// </summary>
    public class CancelOrderRequest
    {
        /// <summary>
        /// Reason for cancellation
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Request model for updating order status
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// New status for the order
        /// </summary>
        public OrderStatus Status { get; set; }
        
        /// <summary>
        /// Notes about the status change
        /// </summary>
        public string? Notes { get; set; }
    }
} 