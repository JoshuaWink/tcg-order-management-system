using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TCGOrderManagement.InventoryService.Configuration;
using TCGOrderManagement.InventoryService.Repositories;
using TCGOrderManagement.InventoryService.Services;

namespace TCGOrderManagement.InventoryService.Extensions
{
    /// <summary>
    /// Extensions for adding MongoDB services to the service collection
    /// </summary>
    public static class MongoDBServiceExtensions
    {
        /// <summary>
        /// Adds MongoDB services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            // Create MongoDB settings from environment variables or configuration
            var settings = new MongoDbSettings
            {
                ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION") ?? 
                                  configuration["MongoDB:ConnectionString"],
                DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE") ?? 
                              configuration["MongoDB:DatabaseName"],
                InventoryCollectionName = Environment.GetEnvironmentVariable("MONGODB_INVENTORY_COLLECTION") ?? 
                                         configuration["MongoDB:InventoryCollectionName"],
                ReservationsCollectionName = Environment.GetEnvironmentVariable("MONGODB_RESERVATIONS_COLLECTION") ?? 
                                           configuration["MongoDB:ReservationsCollectionName"]
            };
            
            // Ensure required settings are available
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is not configured. Please set MONGODB_CONNECTION environment variable or configure in appsettings.json.");
            }
            
            if (string.IsNullOrEmpty(settings.DatabaseName))
            {
                throw new InvalidOperationException("MongoDB database name is not configured. Please set MONGODB_DATABASE environment variable or configure in appsettings.json.");
            }
            
            // Default collection names if not provided
            if (string.IsNullOrEmpty(settings.InventoryCollectionName))
            {
                settings.InventoryCollectionName = "Inventory";
            }
            
            if (string.IsNullOrEmpty(settings.ReservationsCollectionName))
            {
                settings.ReservationsCollectionName = "Reservations";
            }
            
            services.AddSingleton(settings);
            
            // Register MongoDB client and database
            services.AddSingleton<IMongoClient>(sp => new MongoClient(settings.ConnectionString));
            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });
            
            // Register repositories
            services.AddSingleton<IInventoryRepository, MongoInventoryRepository>();
            
            // Register background service for cleanup of expired reservations
            services.AddHostedService<ExpiredReservationCleanupService>();
            
            return services;
        }
    }
} 