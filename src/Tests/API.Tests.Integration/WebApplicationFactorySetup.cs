using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using TCGOrderManagement.API.Tests.Integration.Mocks;

namespace TCGOrderManagement.API.Tests.Integration
{
    public class WebApplicationFactorySetup<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use test appsettings for configuration
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var projectDir = Directory.GetCurrentDirectory();
                var configPath = Path.Combine(projectDir, "appsettings.test.json");
                
                config.AddJsonFile(configPath, optional: true);
                config.AddEnvironmentVariables();
            });
            
            // Replace service registrations with mocks
            builder.ConfigureServices(services =>
            {
                // Remove the real services
                var descriptorsToRemove = services
                    .Where(d => 
                        d.ServiceType.Name.Contains("Repository") ||
                        d.ServiceType.Name.Contains("EventPublisher") ||
                        d.ServiceType.Name.Contains("Service") && 
                        !d.ServiceType.Name.Contains("IWebHost"))
                    .ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }
                
                // Add the mocked services
                services.AddSingleton<TCGOrderManagement.OrderService.Repositories.IOrderRepository, MockOrderRepository>();
                services.AddSingleton<TCGOrderManagement.InventoryService.Repositories.IInventoryRepository, MockInventoryRepository>();
                services.AddSingleton<TCGOrderManagement.OrderService.Events.IEventPublisher, MockEventPublisher>();
                services.AddSingleton<TCGOrderManagement.InventoryService.Events.IEventPublisher, MockEventPublisher>();
                services.AddSingleton<TCGOrderManagement.OrderService.Services.IOrderService, MockOrderService>();
                services.AddSingleton<TCGOrderManagement.InventoryService.Services.IInventoryService, MockInventoryService>();

                // Configure test database if needed
                // services.AddDbContext<AppDbContext>(options =>
                // {
                //     options.UseInMemoryDatabase("TestDatabase");
                // });
            });
        }
    }
} 