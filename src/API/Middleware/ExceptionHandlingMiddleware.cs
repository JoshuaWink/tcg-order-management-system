using System.Net;
using System.Text.Json;
using TCGOrderManagement.Api.Models;

namespace TCGOrderManagement.Api.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the ExceptionHandlingMiddleware
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request processing");
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                UnauthorizedAccessException _ => CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized", exception),
                InvalidOperationException _ => CreateResponse(HttpStatusCode.BadRequest, "Invalid operation", exception),
                ArgumentException _ => CreateResponse(HttpStatusCode.BadRequest, "Invalid argument", exception),
                KeyNotFoundException _ => CreateResponse(HttpStatusCode.NotFound, "Resource not found", exception),
                _ => CreateResponse(HttpStatusCode.InternalServerError, "An error occurred while processing your request", exception)
            };
            
            context.Response.StatusCode = (int)response.statusCode;
            await context.Response.WriteAsync(response.json);
        }
        
        private static (HttpStatusCode statusCode, string json) CreateResponse(HttpStatusCode statusCode, string message, Exception exception)
        {
            var response = ApiResponse<object>.ErrorResponse(message, new List<string> { exception.Message });
            var json = JsonSerializer.Serialize(response);
            return (statusCode, json);
        }
    }
} 