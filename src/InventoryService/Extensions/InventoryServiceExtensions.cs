using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCGOrderManagement.InventoryService.Services;
using TCGOrderManagement.InventoryService.Repositories;
using TCGOrderManagement.InventoryService.Handlers;
using TCGOrderManagement.InventoryService.Events;
using TCGOrderManagement.Shared.Extensions;

namespace TCGOrderManagement.InventoryService.Extensions
{
    /// <summary>
    /// Extensions for adding Inventory service components to the service collection
    /// </summary>
    public static class InventoryServiceExtensions
    {
        /// <summary>
        /// Adds Inventory service components to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddInventoryServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MongoDB is already configured by MongoDBServiceExtensions.AddMongoDb method
            
            // Register services
            services.AddScoped<IInventoryService, Services.InventoryService>();
            
            // Register event handlers
            services.AddScoped<IInventoryEventHandler, InventoryEventHandler>();
            
            // Add RabbitMQ messaging
            services.AddRabbitMqMessaging(configuration);
            
            return services;
        }
    }
} 