using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Service for securely hashing and verifying passwords using BCrypt
    /// </summary>
    public interface IPasswordHashingService
    {
        /// <summary>
        /// Hashes a password using BCrypt with a random salt
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The hashed password with embedded salt</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies if a plain text password matches a hashed password
        /// </summary>
        /// <param name="password">The plain text password to check</param>
        /// <param name="hashedPassword">The hashed password to compare against</param>
        /// <returns>True if the password matches, false otherwise</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// Implementation of IPasswordHashingService using BCrypt
    /// </summary>
    public class PasswordHashingService : IPasswordHashingService
    {
        private readonly ILogger<PasswordHashingService> _logger;
        private const int WorkFactor = 12; // Higher work factor = more secure but slower

        public PasswordHashingService(ILogger<PasswordHashingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Generate a password hash using BCrypt with a random salt and specified work factor
                return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while hashing password");
                throw new CryptographicException("Failed to hash password", ex);
            }
        }

        /// <inheritdoc />
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Hashed password cannot be null or empty", nameof(hashedPassword));

            try
            {
                // Verify the password against the stored hash
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false; // Return false on error rather than throwing exception for security
            }
        }
    }
} 