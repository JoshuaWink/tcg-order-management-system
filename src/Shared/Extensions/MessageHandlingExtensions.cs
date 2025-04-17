using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.Shared.Messaging;

namespace TCGOrderManagement.Shared.Extensions
{
    /// <summary>
    /// Extensions for adding message handling services to the service collection
    /// </summary>
    public static class MessageHandlingExtensions
    {
        /// <summary>
        /// Adds message handling services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqConfig = new RabbitMqConfig
            {
                Host = GetRequiredEnvironmentVariable("RABBITMQ_HOST"),
                Port = GetEnvironmentVariableAsInt("RABBITMQ_PORT"),
                Username = GetRequiredEnvironmentVariable("RABBITMQ_USERNAME"),
                Password = GetRequiredEnvironmentVariable("RABBITMQ_PASSWORD"),
                VirtualHost = GetRequiredEnvironmentVariable("RABBITMQ_VHOST"),
                ExchangeName = GetRequiredEnvironmentVariable("RABBITMQ_EXCHANGE")
            };

            // Validate and register the configuration
            rabbitMqConfig.Validate();
            services.AddSingleton(rabbitMqConfig);

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<RabbitMqEventPublisher>>();
            logger.LogInformation("RabbitMQ configured with Host={Host}, Port={Port}, VirtualHost={VirtualHost}, Exchange={Exchange}",
                rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, rabbitMqConfig.ExchangeName);

            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
            
            // Add other messaging-related services as needed
            
            return services;
        }

        /// <summary>
        /// Gets a required environment variable, throwing an exception if it doesn't exist
        /// </summary>
        private static string GetRequiredEnvironmentVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Required environment variable '{name}' is not set");
            }
            return value;
        }

        /// <summary>
        /// Gets an environment variable as an integer, throwing an exception if it doesn't exist or is not a valid integer
        /// </summary>
        private static int GetEnvironmentVariableAsInt(string name)
        {
            var value = GetRequiredEnvironmentVariable(name);
            if (!int.TryParse(value, out int result))
            {
                throw new InvalidOperationException($"Environment variable '{name}' is not a valid integer");
            }
            return result;
        }
    }
} 