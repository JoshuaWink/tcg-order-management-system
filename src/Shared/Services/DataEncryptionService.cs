using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TCGOrderManagement.Shared.Services
{
    /// <summary>
    /// Interface for data encryption service to protect sensitive data at rest
    /// </summary>
    public interface IDataEncryptionService
    {
        /// <summary>
        /// Encrypts sensitive data using AES-256
        /// </summary>
        /// <param name="plainText">The plain text data to encrypt</param>
        /// <returns>Base64-encoded encrypted data</returns>
        string Encrypt(string plainText);
        
        /// <summary>
        /// Decrypts data that was encrypted with the Encrypt method
        /// </summary>
        /// <param name="cipherText">Base64-encoded encrypted data</param>
        /// <returns>The original plain text</returns>
        string Decrypt(string cipherText);
        
        /// <summary>
        /// Determines if a string is encrypted (has the expected format)
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>True if the text appears to be encrypted, false otherwise</returns>
        bool IsEncrypted(string text);
    }
    
    /// <summary>
    /// Implementation of data encryption service using AES-256 for encrypting sensitive data at rest
    /// </summary>
    public class DataEncryptionService : IDataEncryptionService
    {
        private readonly ILogger<DataEncryptionService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _initializationVector;
        private const string EncryptionPrefix = "ENC:"; // Prefix to identify encrypted values
        
        /// <summary>
        /// Initializes a new instance of the DataEncryptionService class
        /// </summary>
        /// <param name="configuration">Configuration to get encryption keys</param>
        /// <param name="logger">Logger for error reporting</param>
        public DataEncryptionService(IConfiguration configuration, ILogger<DataEncryptionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // In a production environment, these would come from a secure key management system
            // like Azure Key Vault, AWS KMS, or HashiCorp Vault
            string encryptionKeyString = Environment.GetEnvironmentVariable("TCG_ENCRYPTION_KEY") 
                ?? configuration["Encryption:Key"] 
                ?? throw new InvalidOperationException("Encryption key not configured");
                
            string ivString = Environment.GetEnvironmentVariable("TCG_ENCRYPTION_IV") 
                ?? configuration["Encryption:IV"] 
                ?? throw new InvalidOperationException("Encryption IV not configured");
            
            // Convert strings to byte arrays (the key should be exactly 32 bytes for AES-256)
            _encryptionKey = Convert.FromBase64String(encryptionKeyString);
            _initializationVector = Convert.FromBase64String(ivString);
            
            // Validate key length for AES-256
            if (_encryptionKey.Length != 32)
            {
                throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) for AES-256");
            }
            
            // Validate IV length
            if (_initializationVector.Length != 16)
            {
                throw new InvalidOperationException("Initialization vector must be 16 bytes (128 bits)");
            }
        }
        
        /// <inheritdoc />
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
                
            if (IsEncrypted(plainText))
                return plainText; // Already encrypted
            
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _initializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                        
                        byte[] encryptedBytes = memoryStream.ToArray();
                        return EncryptionPrefix + Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new CryptographicException("Failed to encrypt data", ex);
            }
        }
        
        /// <inheritdoc />
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
                
            // If the cipherText doesn't have our prefix, it's not encrypted by us
            if (!IsEncrypted(cipherText))
                return cipherText;
                
            try
            {
                // Remove the prefix
                string actualCipherText = cipherText.Substring(EncryptionPrefix.Length);
                byte[] cipherBytes = Convert.FromBase64String(actualCipherText);
                
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _initializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                throw new CryptographicException("Failed to decrypt data", ex);
            }
        }
        
        /// <inheritdoc />
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(EncryptionPrefix);
        }
    }
} 