using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCGOrderManagement.OrderService.Services;

namespace TCGOrderManagement.OrderService.Extensions
{
    /// <summary>
    /// Extension methods for adding order services to the service collection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds order services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register order services
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentDetailsService, PaymentDetailsService>();
            
            return services;
        }
    }
} 