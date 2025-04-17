using TCGOrderManagement.Api.Models;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Service for user management
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>User information if authentication is successful, null otherwise</returns>
        Task<UserDto?> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="registerRequest">Registration information</param>
        /// <returns>User information if registration is successful</returns>
        Task<UserDto> RegisterAsync(RegisterRequest registerRequest);
        
        /// <summary>
        /// Gets user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information if found, null otherwise</returns>
        Task<UserDto?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Updates a user's refresh token
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="refreshTokenExpiryTime">Expiry time for the refresh token</param>
        /// <returns>True if update is successful, false otherwise</returns>
        Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime refreshTokenExpiryTime);
    }
} 