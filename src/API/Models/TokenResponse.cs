namespace TCGOrderManagement.Api.Models
{
    /// <summary>
    /// Data transfer object for token responses
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Token expiration timestamp
        /// </summary>
        public DateTime Expiration { get; set; }
        
        /// <summary>
        /// User information
        /// </summary>
        public UserDto User { get; set; } = new UserDto();
    }
} 