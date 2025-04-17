using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Interface for account lockout service
    /// </summary>
    public interface IAccountLockoutService
    {
        /// <summary>
        /// Records a failed login attempt
        /// </summary>
        /// <param name="username">The username that failed to login</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RecordFailedLoginAttemptAsync(string username);
        
        /// <summary>
        /// Resets failed login attempts for a user
        /// </summary>
        /// <param name="username">The username to reset attempts for</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ResetFailedLoginAttemptsAsync(string username);
        
        /// <summary>
        /// Checks if a user is locked out due to too many failed login attempts
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if the user is locked out, false otherwise</returns>
        Task<bool> IsUserLockedOutAsync(string username);
    }
    
    /// <summary>
    /// Implementation of account lockout service
    /// </summary>
    public class AccountLockoutService : IAccountLockoutService
    {
        private readonly ILogger<AccountLockoutService> _logger;
        private readonly string _connectionString;
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutMinutes;
        
        /// <summary>
        /// Initializes a new instance of the AccountLockoutService
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public AccountLockoutService(ILogger<AccountLockoutService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // In production, this would use environment variables or key vault
            _connectionString = Environment.GetEnvironmentVariable("TCG_AUTH_DB_CONNECTION") 
                ?? configuration.GetConnectionString("AuthDatabase");
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("No connection string found for Auth Database");
                _connectionString = "Server=db-server;Database=TCGAuthDb;User Id=${DB_USER};Password=${DB_PASSWORD};";
            }
            
            // Get account lockout settings from configuration or use defaults
            _maxFailedAttempts = configuration.GetValue<int>("Security:MaxFailedAttempts", 5);
            _lockoutMinutes = configuration.GetValue<int>("Security:LockoutMinutes", 15);
        }
        
        /// <inheritdoc />
        public async Task RecordFailedLoginAttemptAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
                
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // First, check if there's an entry for this user
                using var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM FailedLoginAttempts WHERE Username = @Username", connection);
                checkCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                
                var entryExists = (int)await checkCommand.ExecuteScalarAsync() > 0;
                
                if (entryExists)
                {
                    // Update existing record
                    using var updateCommand = new SqlCommand(
                        @"UPDATE FailedLoginAttempts 
                          SET FailedCount = FailedCount + 1, 
                              LastFailedAttempt = @LastFailedAttempt 
                          WHERE Username = @Username", connection);
                    updateCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                    updateCommand.Parameters.Add("@LastFailedAttempt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    
                    await updateCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    // Insert new record
                    using var insertCommand = new SqlCommand(
                        @"INSERT INTO FailedLoginAttempts (Username, FailedCount, LastFailedAttempt)
                          VALUES (@Username, 1, @LastFailedAttempt)", connection);
                    insertCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                    insertCommand.Parameters.Add("@LastFailedAttempt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    
                    await insertCommand.ExecuteNonQueryAsync();
                }
                
                _logger.LogWarning("Failed login attempt for user {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failed login attempt for {Username}", username);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task ResetFailedLoginAttemptsAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
                
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    "UPDATE FailedLoginAttempts SET FailedCount = 0 WHERE Username = @Username", connection);
                command.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                
                await command.ExecuteNonQueryAsync();
                
                _logger.LogInformation("Reset failed login attempts for user {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed login attempts for {Username}", username);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> IsUserLockedOutAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
                
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    @"SELECT FailedCount, LastFailedAttempt 
                      FROM FailedLoginAttempts 
                      WHERE Username = @Username", connection);
                command.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    return false; // No failed attempts record exists
                }
                
                var failedCount = reader.GetInt32(0);
                var lastFailedAttempt = reader.GetDateTime(1);
                
                // If user has exceeded maximum attempts and the lockout period hasn't expired
                if (failedCount >= _maxFailedAttempts)
                {
                    var lockoutExpiry = lastFailedAttempt.AddMinutes(_lockoutMinutes);
                    var isLockedOut = DateTime.UtcNow < lockoutExpiry;
                    
                    if (isLockedOut)
                    {
                        _logger.LogWarning("User {Username} is locked out until {LockoutExpiry}", 
                            username, lockoutExpiry);
                    }
                    else
                    {
                        // Lockout period has expired, reset the counter
                        await ResetFailedLoginAttemptsAsync(username);
                    }
                    
                    return isLockedOut;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lockout status for {Username}", username);
                return false; // Default to not locked out in case of error
            }
        }
    }
} 