using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TCGOrderManagement.InventoryService.Configuration;
using TCGOrderManagement.InventoryService.Repositories;

namespace TCGOrderManagement.InventoryService.Services
{
    /// <summary>
    /// Background service that periodically cleans up expired inventory reservations
    /// </summary>
    public class ExpiredReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredReservationCleanupService> _logger;
        private readonly TimeSpan _interval;

        /// <summary>
        /// Initializes a new instance of the ExpiredReservationCleanupService class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        public ExpiredReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Run cleanup every 5 minutes
            _interval = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Executes the background task
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired reservation cleanup service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredReservationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during expired reservation cleanup");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Expired reservation cleanup service is stopping");
        }

        /// <summary>
        /// Performs the cleanup of expired reservations
        /// </summary>
        private async Task CleanupExpiredReservationsAsync()
        {
            _logger.LogDebug("Starting cleanup of expired reservations");

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

            try
            {
                int cleanedCount = await repository.CleanupExpiredReservationsAsync();
                
                if (cleanedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired inventory reservations", cleanedCount);
                }
                else
                {
                    _logger.LogDebug("No expired reservations found to clean up");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired reservations");
                throw;
            }
        }
    }
} 