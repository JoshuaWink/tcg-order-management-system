using System;

namespace TCGOrderManagement.API.Models
{
    /// <summary>
    /// Represents a record of failed login attempts for a user
    /// </summary>
    public class FailedLoginAttempt
    {
        /// <summary>
        /// Unique identifier for the failed login attempt record
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Username for which the failed attempts are being tracked
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Number of consecutive failed login attempts
        /// </summary>
        public int FailedCount { get; set; }
        
        /// <summary>
        /// Timestamp of the most recent failed attempt
        /// </summary>
        public DateTime LastFailedAttempt { get; set; }
        
        /// <summary>
        /// Determines if the account is currently locked out based on the system settings
        /// </summary>
        /// <param name="maxFailedAttempts">Maximum number of failed attempts allowed</param>
        /// <param name="lockoutDurationMinutes">Duration of lockout in minutes</param>
        /// <returns>True if the account is locked out, false otherwise</returns>
        public bool IsLockedOut(int maxFailedAttempts, int lockoutDurationMinutes)
        {
            if (FailedCount < maxFailedAttempts)
            {
                return false;
            }
            
            var lockoutEndTime = LastFailedAttempt.AddMinutes(lockoutDurationMinutes);
            return DateTime.UtcNow < lockoutEndTime;
        }
    }
} 