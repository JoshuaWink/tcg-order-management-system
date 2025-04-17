using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.OrderService.Extensions;

namespace TCGOrderManagement.Shared.Messaging
{
    /// <summary>
    /// Background service that consumes messages from the message bus
    /// </summary>
    public class MessageConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageConsumerService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the MessageConsumerService class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="messageBus">The message bus</param>
        /// <param name="logger">The logger</param>
        public MessageConsumerService(
            IServiceProvider serviceProvider,
            IMessageBus messageBus,
            ILogger<MessageConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _messageBus = messageBus;
            _logger = logger;
        }
        
        /// <summary>
        /// Executes the background task
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message consumer service is starting");
            
            // Subscribe to message types
            _messageBus.Subscribe<object>("order.created", HandleOrderCreated);
            _messageBus.Subscribe<object>("payment.processed", HandlePaymentProcessed);
            _messageBus.Subscribe<object>("inventory.reserved", HandleInventoryReserved);
            _messageBus.Subscribe<object>("order.shipped", HandleOrderShipped);
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
            
            _logger.LogInformation("Message consumer service is stopping");
        }
        
        private async Task HandleOrderCreated(object message)
        {
            _logger.LogInformation("Handling order.created event");
            
            using var scope = _serviceProvider.CreateScope();
            
            // Process the event
            // In a real implementation, we would deserialize the message and call the appropriate handler
            await Task.CompletedTask;
        }
        
        private async Task HandlePaymentProcessed(object message)
        {
            _logger.LogInformation("Handling payment.processed event");
            
            using var scope = _serviceProvider.CreateScope();
            
            // Process the event
            await Task.CompletedTask;
        }
        
        private async Task HandleInventoryReserved(object message)
        {
            _logger.LogInformation("Handling inventory.reserved event");
            
            using var scope = _serviceProvider.CreateScope();
            
            // Process the event
            await Task.CompletedTask;
        }
        
        private async Task HandleOrderShipped(object message)
        {
            _logger.LogInformation("Handling order.shipped event");
            
            using var scope = _serviceProvider.CreateScope();
            
            // Process the event
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Message consumer service is stopping");
            
            await base.StopAsync(cancellationToken);
        }
    }
} 