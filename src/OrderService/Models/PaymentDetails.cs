using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TCGOrderManagement.Shared.Services;

namespace TCGOrderManagement.OrderService.Models
{
    /// <summary>
    /// Represents payment details with encrypted sensitive fields
    /// </summary>
    public class PaymentDetails
    {
        /// <summary>
        /// Unique identifier for the payment details
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Associated user ID
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType { get; set; }
        
        /// <summary>
        /// Name on the payment method (encrypted at rest)
        /// </summary>
        public string CardholderName { get; private set; }
        
        /// <summary>
        /// Masked card number (only last 4 digits are stored)
        /// </summary>
        public string MaskedCardNumber { get; set; }
        
        /// <summary>
        /// Expiration month
        /// </summary>
        public int ExpirationMonth { get; set; }
        
        /// <summary>
        /// Expiration year
        /// </summary>
        public int ExpirationYear { get; set; }
        
        /// <summary>
        /// Billing address (encrypted at rest)
        /// </summary>
        public string BillingAddress { get; private set; }
        
        /// <summary>
        /// Date the payment method was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Date the payment method was last updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        
        /// <summary>
        /// Token from payment processor (encrypted at rest)
        /// </summary>
        public string PaymentToken { get; private set; }
        
        /// <summary>
        /// Sets the cardholder name with encryption
        /// </summary>
        /// <param name="name">Plain text cardholder name</param>
        /// <param name="encryptionService">Encryption service</param>
        public void SetCardholderName(string name, IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(name))
                return;
                
            CardholderName = encryptionService.Encrypt(name);
        }
        
        /// <summary>
        /// Gets the decrypted cardholder name
        /// </summary>
        /// <param name="encryptionService">Encryption service</param>
        /// <returns>Decrypted cardholder name</returns>
        public string GetCardholderName(IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(CardholderName))
                return string.Empty;
                
            return encryptionService.Decrypt(CardholderName);
        }
        
        /// <summary>
        /// Sets the billing address with encryption
        /// </summary>
        /// <param name="address">Plain text billing address</param>
        /// <param name="encryptionService">Encryption service</param>
        public void SetBillingAddress(string address, IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(address))
                return;
                
            BillingAddress = encryptionService.Encrypt(address);
        }
        
        /// <summary>
        /// Gets the decrypted billing address
        /// </summary>
        /// <param name="encryptionService">Encryption service</param>
        /// <returns>Decrypted billing address</returns>
        public string GetBillingAddress(IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(BillingAddress))
                return string.Empty;
                
            return encryptionService.Decrypt(BillingAddress);
        }
        
        /// <summary>
        /// Sets the payment token with encryption
        /// </summary>
        /// <param name="token">Plain text payment token</param>
        /// <param name="encryptionService">Encryption service</param>
        public void SetPaymentToken(string token, IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(token))
                return;
                
            PaymentToken = encryptionService.Encrypt(token);
        }
        
        /// <summary>
        /// Gets the decrypted payment token
        /// </summary>
        /// <param name="encryptionService">Encryption service</param>
        /// <returns>Decrypted payment token</returns>
        public string GetPaymentToken(IDataEncryptionService encryptionService)
        {
            if (string.IsNullOrEmpty(PaymentToken))
                return string.Empty;
                
            return encryptionService.Decrypt(PaymentToken);
        }
    }
    
    /// <summary>
    /// Types of payment methods
    /// </summary>
    public enum PaymentMethodType
    {
        /// <summary>
        /// Credit card
        /// </summary>
        CreditCard = 1,
        
        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 2,
        
        /// <summary>
        /// Bank transfer
        /// </summary>
        BankTransfer = 3,
        
        /// <summary>
        /// Store credit
        /// </summary>
        StoreCredit = 4
    }
} 