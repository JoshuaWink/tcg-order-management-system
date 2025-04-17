using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TCGOrderManagement.API.Models;

namespace TCGOrderManagement.API.Repositories
{
    public interface IFailedLoginRepository
    {
        Task<FailedLoginAttempt> GetByUsernameAsync(string username);
        Task CreateAsync(FailedLoginAttempt attempt);
        Task UpdateAsync(FailedLoginAttempt attempt);
        Task ResetAttemptsAsync(string username);
    }

    public class FailedLoginRepository : IFailedLoginRepository
    {
        private readonly string _connectionString;

        public FailedLoginRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<FailedLoginAttempt> GetByUsernameAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand("SELECT Id, Username, FailedCount, LastFailedAttempt FROM FailedLoginAttempts WHERE Username = @Username", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new FailedLoginAttempt
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                FailedCount = reader.GetInt32(2),
                                LastFailedAttempt = reader.GetDateTime(3)
                            };
                        }
                        
                        return null;
                    }
                }
            }
        }

        public async Task CreateAsync(FailedLoginAttempt attempt)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(
                    "INSERT INTO FailedLoginAttempts (Username, FailedCount, LastFailedAttempt) " +
                    "VALUES (@Username, @FailedCount, @LastFailedAttempt)", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = attempt.Username;
                    command.Parameters.Add("@FailedCount", SqlDbType.Int).Value = attempt.FailedCount;
                    command.Parameters.Add("@LastFailedAttempt", SqlDbType.DateTime2).Value = attempt.LastFailedAttempt;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateAsync(FailedLoginAttempt attempt)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(
                    "UPDATE FailedLoginAttempts SET FailedCount = @FailedCount, LastFailedAttempt = @LastFailedAttempt " +
                    "WHERE Username = @Username", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = attempt.Username;
                    command.Parameters.Add("@FailedCount", SqlDbType.Int).Value = attempt.FailedCount;
                    command.Parameters.Add("@LastFailedAttempt", SqlDbType.DateTime2).Value = attempt.LastFailedAttempt;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ResetAttemptsAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(
                    "UPDATE FailedLoginAttempts SET FailedCount = 0 WHERE Username = @Username", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 