using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.API.Models;

namespace TCGOrderManagement.API.Database.Repositories
{
    public class FailedLoginAttemptsRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<FailedLoginAttemptsRepository> _logger;

        public FailedLoginAttemptsRepository(IDbConnection dbConnection, ILogger<FailedLoginAttemptsRepository> logger)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FailedLoginAttempt> GetByUsernameAsync(string username)
        {
            try
            {
                const string sql = @"
                    SELECT Id, Username, FailedCount, LastFailedAttempt 
                    FROM FailedLoginAttempts 
                    WHERE Username = @Username";

                return await _dbConnection.QueryFirstOrDefaultAsync<FailedLoginAttempt>(sql, new { Username = username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed login attempts for user {Username}", username);
                throw;
            }
        }

        public async Task UpsertFailedLoginAttemptAsync(string username, int failedCount)
        {
            try
            {
                const string sql = @"
                    MERGE INTO FailedLoginAttempts AS target
                    USING (SELECT @Username AS Username) AS source
                    ON target.Username = source.Username
                    WHEN MATCHED THEN
                        UPDATE SET 
                            FailedCount = @FailedCount,
                            LastFailedAttempt = @LastFailedAttempt
                    WHEN NOT MATCHED THEN
                        INSERT (Username, FailedCount, LastFailedAttempt)
                        VALUES (@Username, @FailedCount, @LastFailedAttempt);";

                await _dbConnection.ExecuteAsync(sql, new
                {
                    Username = username,
                    FailedCount = failedCount,
                    LastFailedAttempt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating failed login attempts for user {Username}", username);
                throw;
            }
        }

        public async Task ResetFailedAttemptsAsync(string username)
        {
            try
            {
                const string sql = @"
                    DELETE FROM FailedLoginAttempts
                    WHERE Username = @Username";

                await _dbConnection.ExecuteAsync(sql, new { Username = username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed login attempts for user {Username}", username);
                throw;
            }
        }
    }
} 