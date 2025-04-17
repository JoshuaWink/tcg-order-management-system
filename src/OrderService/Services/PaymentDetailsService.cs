using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.OrderService.Models;
using TCGOrderManagement.Shared.Services;

namespace TCGOrderManagement.OrderService.Services
{
    /// <summary>
    /// Interface for payment details service
    /// </summary>
    public interface IPaymentDetailsService
    {
        /// <summary>
        /// Gets payment details by ID
        /// </summary>
        /// <param name="id">Payment details ID</param>
        /// <returns>Payment details if found, null otherwise</returns>
        Task<PaymentDetails> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Saves payment details
        /// </summary>
        /// <param name="paymentDetails">Payment details to save</param>
        /// <returns>Saved payment details</returns>
        Task<PaymentDetails> SaveAsync(PaymentDetails paymentDetails);
        
        /// <summary>
        /// Deletes payment details
        /// </summary>
        /// <param name="id">Payment details ID</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> DeleteAsync(Guid id);
    }
    
    /// <summary>
    /// Implementation of payment details service with encrypted field handling
    /// </summary>
    public class PaymentDetailsService : IPaymentDetailsService
    {
        private readonly ILogger<PaymentDetailsService> _logger;
        private readonly string _connectionString;
        private readonly IDataEncryptionService _encryptionService;
        
        /// <summary>
        /// Initializes a new instance of PaymentDetailsService
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="encryptionService">Data encryption service</param>
        public PaymentDetailsService(
            ILogger<PaymentDetailsService> logger,
            IConfiguration configuration,
            IDataEncryptionService encryptionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            
            _connectionString = configuration.GetConnectionString("OrderDatabase")
                ?? throw new InvalidOperationException("Order database connection string not found");
        }
        
        /// <inheritdoc />
        public async Task<PaymentDetails> GetByIdAsync(Guid id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(@"
                    SELECT Id, UserId, PaymentMethodType, CardholderName, MaskedCardNumber,
                           ExpirationMonth, ExpirationYear, BillingAddress, CreatedDate,
                           UpdatedDate, PaymentToken
                    FROM PaymentDetails
                    WHERE Id = @Id", connection);
                    
                command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var paymentDetails = new PaymentDetails
                    {
                        Id = reader.GetGuid(0),
                        UserId = reader.GetGuid(1),
                        PaymentMethodType = (PaymentMethodType)reader.GetInt32(2),
                        // CardholderName is encrypted and handled through getter/setter
                        MaskedCardNumber = reader.GetString(4),
                        ExpirationMonth = reader.GetInt32(5),
                        ExpirationYear = reader.GetInt32(6),
                        // BillingAddress is encrypted and handled through getter/setter
                        CreatedDate = reader.GetDateTime(8),
                        UpdatedDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                        // PaymentToken is encrypted and handled through getter/setter
                    };
                    
                    // Set the encrypted fields using the original values from database
                    // The getters will automatically decrypt these when needed
                    if (!reader.IsDBNull(3))
                    {
                        var encryptedCardholderName = reader.GetString(3);
                        paymentDetails.SetCardholderName(
                            _encryptionService.Decrypt(encryptedCardholderName), 
                            _encryptionService);
                    }
                    
                    if (!reader.IsDBNull(7))
                    {
                        var encryptedBillingAddress = reader.GetString(7);
                        paymentDetails.SetBillingAddress(
                            _encryptionService.Decrypt(encryptedBillingAddress), 
                            _encryptionService);
                    }
                    
                    if (!reader.IsDBNull(10))
                    {
                        var encryptedPaymentToken = reader.GetString(10);
                        paymentDetails.SetPaymentToken(
                            _encryptionService.Decrypt(encryptedPaymentToken), 
                            _encryptionService);
                    }
                    
                    return paymentDetails;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment details with ID {PaymentDetailsId}", id);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<PaymentDetails> SaveAsync(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null)
                throw new ArgumentNullException(nameof(paymentDetails));
                
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Check if this is an insert or update
                var isUpdate = paymentDetails.Id != Guid.Empty;
                
                if (!isUpdate)
                {
                    // Generate a new ID for new records
                    paymentDetails.Id = Guid.NewGuid();
                }
                
                string sql = isUpdate
                    ? @"
                        UPDATE PaymentDetails SET
                            UserId = @UserId,
                            PaymentMethodType = @PaymentMethodType,
                            CardholderName = @CardholderName,
                            MaskedCardNumber = @MaskedCardNumber,
                            ExpirationMonth = @ExpirationMonth,
                            ExpirationYear = @ExpirationYear,
                            BillingAddress = @BillingAddress,
                            UpdatedDate = @UpdatedDate,
                            PaymentToken = @PaymentToken
                        WHERE Id = @Id"
                    : @"
                        INSERT INTO PaymentDetails (
                            Id, UserId, PaymentMethodType, CardholderName,
                            MaskedCardNumber, ExpirationMonth, ExpirationYear,
                            BillingAddress, CreatedDate, UpdatedDate, PaymentToken
                        ) VALUES (
                            @Id, @UserId, @PaymentMethodType, @CardholderName,
                            @MaskedCardNumber, @ExpirationMonth, @ExpirationYear,
                            @BillingAddress, @CreatedDate, @UpdatedDate, @PaymentToken
                        )";
                
                using var command = new SqlCommand(sql, connection);
                
                // Add parameters
                command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = paymentDetails.Id;
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = paymentDetails.UserId;
                command.Parameters.Add("@PaymentMethodType", SqlDbType.Int).Value = (int)paymentDetails.PaymentMethodType;
                
                // Encrypted fields - these are already encrypted through the setter methods
                AddParameterWithNullCheck(command, "@CardholderName", SqlDbType.NVarChar, paymentDetails.CardholderName);
                AddParameterWithNullCheck(command, "@MaskedCardNumber", SqlDbType.NVarChar, paymentDetails.MaskedCardNumber);
                command.Parameters.Add("@ExpirationMonth", SqlDbType.Int).Value = paymentDetails.ExpirationMonth;
                command.Parameters.Add("@ExpirationYear", SqlDbType.Int).Value = paymentDetails.ExpirationYear;
                AddParameterWithNullCheck(command, "@BillingAddress", SqlDbType.NVarChar, paymentDetails.BillingAddress);
                
                // Dates
                if (isUpdate)
                {
                    // For updates, only set the UpdatedDate
                    command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                }
                else
                {
                    // For inserts, set both CreatedDate and UpdatedDate
                    command.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime2).Value = DBNull.Value;
                }
                
                AddParameterWithNullCheck(command, "@PaymentToken", SqlDbType.NVarChar, paymentDetails.PaymentToken);
                
                // Execute the command
                await command.ExecuteNonQueryAsync();
                
                // Retrieve and return the saved entity
                return await GetByIdAsync(paymentDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment details for user {UserId}", paymentDetails.UserId);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    "DELETE FROM PaymentDetails WHERE Id = @Id", connection);
                command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment details with ID {PaymentDetailsId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Adds a parameter to the command with null check
        /// </summary>
        private void AddParameterWithNullCheck(SqlCommand command, string parameterName, SqlDbType sqlType, object value)
        {
            if (value == null)
            {
                command.Parameters.Add(parameterName, sqlType).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add(parameterName, sqlType).Value = value;
            }
        }
    }
} 