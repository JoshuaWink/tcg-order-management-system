using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCGOrderManagement.Api.Models;
using TCGOrderManagement.Api.Services;

namespace TCGOrderManagement.Api.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="userService">User service</param>
        /// <param name="tokenService">Token service</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
        }
        
        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Token response</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid username or password"));
            }
            
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Calculate expiry time for refresh token (e.g., 7 days)
            var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            // Store refresh token
            await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiryTime);
            
            // Calculate access token expiry time from configuration
            var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);
            var accessTokenExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);
            
            var response = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = accessTokenExpiryTime,
                User = user
            };
            
            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            
            return Ok(ApiResponse<TokenResponse>.SuccessResponse(response, "Login successful"));
        }
        
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">Registration request</param>
        /// <returns>Token response</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterAsync(request);
                
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                
                // Calculate expiry time for refresh token (e.g., 7 days)
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                
                // Store refresh token
                await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiryTime);
                
                // Calculate access token expiry time from configuration
                var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);
                var accessTokenExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);
                
                var response = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Expiration = accessTokenExpiryTime,
                    User = user
                };
                
                _logger.LogInformation("User {Username} registered successfully", user.Username);
                
                return Ok(ApiResponse<TokenResponse>.SuccessResponse(response, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for username {Username}", request.Username);
                return BadRequest(ApiResponse<object>.ErrorResponse("Registration failed", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>Token response</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required"));
            }
            
            // Validate refresh token
            var isValid = _tokenService.ValidateRefreshToken(request.RefreshToken);
            if (!isValid)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid refresh token"));
            }
            
            // Extract user ID from access token
            var userId = _tokenService.GetUserIdFromToken(request.AccessToken);
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid access token"));
            }
            
            // Get user by ID
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not found"));
            }
            
            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Calculate expiry time for refresh token (e.g., 7 days)
            var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            // Store refresh token
            await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiryTime);
            
            // Calculate access token expiry time from configuration
            var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);
            var accessTokenExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);
            
            var response = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = accessTokenExpiryTime,
                User = user
            };
            
            _logger.LogInformation("Token refreshed for user {Username}", user.Username);
            
            return Ok(ApiResponse<TokenResponse>.SuccessResponse(response, "Token refreshed successfully"));
        }
        
        /// <summary>
        /// Gets user profile information
        /// </summary>
        /// <returns>User information</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            // Extract user ID from token claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }
            
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }
            
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
        }
    }
    
    /// <summary>
    /// Request model for refreshing tokens
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
} 