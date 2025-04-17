using System;
using TCGOrderManagement.Shared.Utilities;

namespace TCGOrderManagement.Tools.KeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TCG Order Management System - Encryption Key Generator");
            Console.WriteLine("======================================================");
            Console.WriteLine();
            
            // Generate and display keys
            EncryptionKeyGenerator.DisplayGeneratedKeys();
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
} 