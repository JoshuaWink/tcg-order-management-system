using TCGOrderManagement.Api.Models;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Service for managing JWT tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the given user
        /// </summary>
        /// <param name="user">User information</param>
        /// <returns>JWT token string</returns>
        string GenerateAccessToken(UserDto user);
        
        /// <summary>
        /// Generates a refresh token for the given user
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
        
        /// <summary>
        /// Validates the provided refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        bool ValidateRefreshToken(string refreshToken);
        
        /// <summary>
        /// Gets user ID from token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID from the token claims</returns>
        Guid? GetUserIdFromToken(string token);
    }
} 