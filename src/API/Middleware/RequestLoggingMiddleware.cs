using System.Diagnostics;
using System.Text;

namespace TCGOrderManagement.Api.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        
        /// <summary>
        /// Initializes a new instance of the RequestLoggingMiddleware
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
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
            // Check if we're in development mode
            if (!context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                // Only log basic info in production
                _logger.LogInformation("{Method} {Path} requested by {IP}", 
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Connection.RemoteIpAddress);
                
                await _next(context);
                return;
            }
            
            // In development, log more details
            var sw = Stopwatch.StartNew();
            var requestBodyContent = await ReadRequestBodyAsync(context.Request);
            
            // Create a response body stream we can read from
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;
            
            try
            {
                await _next(context);
                sw.Stop();
                
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBodyContent = await ReadStreamAsync(responseBodyStream);
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                
                await responseBodyStream.CopyToAsync(originalBodyStream);
                
                _logger.LogInformation(
                    "Request: {Method} {Path} from {IP}\n" +
                    "Request Headers: {Headers}\n" +
                    "Request Body: {RequestBody}\n" +
                    "Response Status: {StatusCode}\n" +
                    "Response Body: {ResponseBody}\n" +
                    "Duration: {Duration}ms",
                    context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress,
                    FormatHeaders(context.Request.Headers),
                    requestBodyContent,
                    context.Response.StatusCode,
                    responseBodyContent,
                    sw.ElapsedMilliseconds);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
        
        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            
            using var streamReader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            
            var requestBody = await streamReader.ReadToEndAsync();
            request.Body.Position = 0;
            
            return requestBody;
        }
        
        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            using var streamReader = new StreamReader(
                stream,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            
            return await streamReader.ReadToEndAsync();
        }
        
        private static string FormatHeaders(IHeaderDictionary headers)
        {
            var sb = new StringBuilder();
            foreach (var (key, value) in headers)
            {
                // Skip sensitive headers
                if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{key}: [REDACTED]");
                }
                else
                {
                    sb.AppendLine($"{key}: {value}");
                }
            }
            
            return sb.ToString();
        }
    }
} 