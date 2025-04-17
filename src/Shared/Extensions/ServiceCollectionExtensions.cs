using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TCGOrderManagement.Shared.Services;

namespace TCGOrderManagement.Shared.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds data protection services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            // Register data encryption service
            services.AddSingleton<IDataEncryptionService, DataEncryptionService>();
            
            return services;
        }
    }
} 