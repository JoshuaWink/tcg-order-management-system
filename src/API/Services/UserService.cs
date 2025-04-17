using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.Api.Models;

namespace TCGOrderManagement.Api.Services
{
    /// <summary>
    /// Implementation of the user service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly string _connectionString;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IAccountLockoutService _accountLockoutService;
        
        /// <summary>
        /// Initializes a new instance of the UserService
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="passwordHashingService">Password hashing service</param>
        /// <param name="accountLockoutService">Account lockout service</param>
        public UserService(
            ILogger<UserService> logger, 
            IConfiguration configuration,
            IPasswordHashingService passwordHashingService,
            IAccountLockoutService accountLockoutService)
        {
            _logger = logger;
            _passwordHashingService = passwordHashingService ?? throw new ArgumentNullException(nameof(passwordHashingService));
            _accountLockoutService = accountLockoutService ?? throw new ArgumentNullException(nameof(accountLockoutService));
            
            // In production, this would use environment variables or key vault
            _connectionString = Environment.GetEnvironmentVariable("TCG_AUTH_DB_CONNECTION") 
                ?? configuration.GetConnectionString("AuthDatabase");
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("No connection string found for Auth Database");
                _connectionString = "Server=db-server;Database=TCGAuthDb;User Id=${DB_USER};Password=${DB_PASSWORD};";
            }
        }
        
        /// <inheritdoc />
        public async Task<UserDto?> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Check if user is locked out
                var isLockedOut = await _accountLockoutService.IsUserLockedOutAsync(username);
                if (isLockedOut)
                {
                    _logger.LogWarning("Account locked out for user {Username}", username);
                    return null;
                }
                
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Get user by username
                using var command = new SqlCommand(
                    "SELECT Id, Username, Email, PasswordHash FROM Users WHERE Username = @Username", connection);
                command.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    // Record failed login attempt for security monitoring
                    await _accountLockoutService.RecordFailedLoginAttemptAsync(username);
                    return null;
                }
                
                var id = reader.GetGuid(0);
                var storedUsername = reader.GetString(1);
                var email = reader.GetString(2);
                var passwordHash = reader.GetString(3);
                
                // Verify password hash
                if (!_passwordHashingService.VerifyPassword(password, passwordHash))
                {
                    await _accountLockoutService.RecordFailedLoginAttemptAsync(username);
                    return null;
                }
                
                // Reset failed login attempts on successful login
                await _accountLockoutService.ResetFailedLoginAttemptsAsync(username);
                
                // Get user roles
                var roles = await GetUserRolesAsync(id);
                
                _logger.LogInformation("User {Username} authenticated successfully", username);
                
                return new UserDto
                {
                    Id = id,
                    Username = storedUsername,
                    Email = email,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Username}", username);
                return null;
            }
        }
        
        /// <inheritdoc />
        public async Task<UserDto> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Check if username or email already exists
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email", connection))
                {
                    checkCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = registerRequest.Username;
                    checkCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = registerRequest.Email;
                    
                    var existingCount = (int)await checkCommand.ExecuteScalarAsync();
                    if (existingCount > 0)
                    {
                        throw new InvalidOperationException("Username or email already exists.");
                    }
                }
                
                // Insert new user
                var userId = Guid.NewGuid();
                
                using (var insertCommand = new SqlCommand(
                    @"INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt) 
                      VALUES (@Id, @Username, @Email, @PasswordHash, @CreatedAt)", connection))
                {
                    insertCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;
                    insertCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = registerRequest.Username;
                    insertCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = registerRequest.Email;
                    insertCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = _passwordHashingService.HashPassword(registerRequest.Password);
                    insertCommand.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    
                    await insertCommand.ExecuteNonQueryAsync();
                }
                
                // Assign default role
                using (var roleCommand = new SqlCommand(
                    @"INSERT INTO UserRoles (UserId, RoleId) 
                      SELECT @UserId, Id FROM Roles WHERE Name = 'Customer'", connection))
                {
                    roleCommand.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    await roleCommand.ExecuteNonQueryAsync();
                }
                
                _logger.LogInformation("User {Username} registered successfully", registerRequest.Username);
                
                return new UserDto
                {
                    Id = userId,
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    Roles = new List<string> { "Customer" } // Default role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", registerRequest.Username);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    "SELECT Id, Username, Email FROM Users WHERE Id = @Id", connection);
                
                command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var userId = reader.GetGuid(0);
                    var username = reader.GetString(1);
                    var email = reader.GetString(2);
                    
                    // Get user roles
                    var roles = await GetUserRolesAsync(userId);
                    
                    return new UserDto
                    {
                        Id = userId,
                        Username = username,
                        Email = email,
                        Roles = roles
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime refreshTokenExpiryTime)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    @"UPDATE Users 
                      SET RefreshToken = @RefreshToken, RefreshTokenExpiryTime = @ExpiryTime 
                      WHERE Id = @UserId", connection);
                
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                command.Parameters.Add("@RefreshToken", SqlDbType.NVarChar).Value = refreshToken;
                command.Parameters.Add("@ExpiryTime", SqlDbType.DateTime2).Value = refreshTokenExpiryTime;
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Refresh token updated for user {UserId}", userId);
                    return true;
                }
                
                _logger.LogWarning("Failed to update refresh token - user {UserId} not found", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refresh token for user {UserId}", userId);
                throw;
            }
        }
        
        /// <summary>
        /// Gets roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of role names</returns>
        private async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var roles = new List<string>();
            
            using var connection = new SqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            using var command = new SqlCommand(
                @"SELECT r.Name 
                  FROM Roles r 
                  INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
                  WHERE ur.UserId = @UserId", connection);
            
            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                roles.Add(reader.GetString(0));
            }
            
            return roles;
        }
    }
} 