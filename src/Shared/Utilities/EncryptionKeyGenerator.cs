using System;
using System.Security.Cryptography;

namespace TCGOrderManagement.Shared.Utilities
{
    /// <summary>
    /// Utility class for generating encryption keys and initialization vectors
    /// This should only be used in development to set up initial keys
    /// In production, keys should be managed by a secure key management system
    /// </summary>
    public static class EncryptionKeyGenerator
    {
        /// <summary>
        /// Generates a random AES-256 key and initialization vector
        /// </summary>
        /// <returns>Tuple containing Base64-encoded key and IV</returns>
        public static (string Key, string IV) GenerateKeyAndIV()
        {
            // Create a new instance of AES
            using (Aes aes = Aes.Create())
            {
                // Set key size to 256 bits
                aes.KeySize = 256;
                
                // Generate a random key and IV
                aes.GenerateKey();
                aes.GenerateIV();
                
                // Convert to Base64 for storage
                string key = Convert.ToBase64String(aes.Key);
                string iv = Convert.ToBase64String(aes.IV);
                
                return (Key: key, IV: iv);
            }
        }
        
        /// <summary>
        /// Displays encryption keys for configuration
        /// </summary>
        public static void DisplayGeneratedKeys()
        {
            var (key, iv) = GenerateKeyAndIV();
            
            Console.WriteLine("Generated AES-256 Encryption Keys");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Key: {key}");
            Console.WriteLine($"IV:  {iv}");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Add these to your appsettings.json or environment variables:");
            Console.WriteLine("\"Encryption\": {");
            Console.WriteLine($"  \"Key\": \"{key}\",");
            Console.WriteLine($"  \"IV\": \"{iv}\"");
            Console.WriteLine("}");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Or set environment variables:");
            Console.WriteLine($"TCG_ENCRYPTION_KEY={key}");
            Console.WriteLine($"TCG_ENCRYPTION_IV={iv}");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("WARNING: Keep these keys secure!");
        }
    }
} 