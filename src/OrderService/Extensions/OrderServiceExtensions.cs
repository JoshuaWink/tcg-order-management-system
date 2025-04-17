using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCGOrderManagement.OrderService.Services;
using TCGOrderManagement.OrderService.Repositories;
using TCGOrderManagement.OrderService.Handlers;
using TCGOrderManagement.OrderService.Events;
using TCGOrderManagement.Shared.Extensions;

namespace TCGOrderManagement.OrderService.Extensions
{
    /// <summary>
    /// Extensions for adding Order service components to the service collection
    /// </summary>
    public static class OrderServiceExtensions
    {
        /// <summary>
        /// Adds Order service components to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database connection
            var connectionString = Environment.GetEnvironmentVariable("ORDERS_DB_CONNECTION")
                ?? configuration.GetConnectionString("OrdersDatabase");
                
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Orders database connection string is not configured. Please set ORDERS_DB_CONNECTION environment variable or configure in appsettings.json.");
            }
            
            // Register repositories
            services.AddSingleton<IOrderRepository>(sp => new SqlOrderRepository(connectionString));
            
            // Register services
            services.AddScoped<IOrderService, Services.OrderService>();
            
            // Register event handlers
            services.AddScoped<IOrderEventHandler, OrderEventHandler>();
            
            // Add RabbitMQ messaging
            services.AddRabbitMqMessaging(configuration);
            
            return services;
        }
    }
} 