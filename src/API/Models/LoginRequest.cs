using System.ComponentModel.DataAnnotations;

namespace TCGOrderManagement.Api.Models
{
    /// <summary>
    /// Data transfer object for login requests
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
} 