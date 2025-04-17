namespace TCGOrderManagement.Api.Models
{
    /// <summary>
    /// Data transfer object for user information
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// User roles
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
} 