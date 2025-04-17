using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TCGOrderManagement.API.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.API.Tests.Integration
{
    [TestClass]
    public class OrderControllerIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Configure test services here if needed
                    // builder.ConfigureServices(services => { });
                });

            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [TestMethod]
        public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
        {
            // Arrange
            var orderRequest = new OrderCreateRequest
            {
                CustomerId = "customer123",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ItemId = "card456",
                        Quantity = 1,
                        RequestedCondition = ItemCondition.NearMint
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var order = await response.Content.ReadFromJsonAsync<Order>();
            order.Should().NotBeNull();
            order.CustomerId.Should().Be(orderRequest.CustomerId);
            order.Items.Should().HaveCount(1);
            order.Status.Should().Be(OrderStatus.Created);
        }

        [TestMethod]
        public async Task GetOrder_ExistingOrder_ReturnsOrder()
        {
            // Arrange - Create an order first
            var orderRequest = new OrderCreateRequest
            {
                CustomerId = "customer456",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ItemId = "card789",
                        Quantity = 2,
                        RequestedCondition = ItemCondition.Mint
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/orders", orderRequest);
            createResponse.EnsureSuccessStatusCode();
            var createdOrder = await createResponse.Content.ReadFromJsonAsync<Order>();
            createdOrder.Should().NotBeNull();
            
            // Act
            var getResponse = await _client.GetAsync($"/api/orders/{createdOrder.Id}");
            
            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var retrievedOrder = await getResponse.Content.ReadFromJsonAsync<Order>();
            retrievedOrder.Should().NotBeNull();
            retrievedOrder.Id.Should().Be(createdOrder.Id);
            retrievedOrder.CustomerId.Should().Be(orderRequest.CustomerId);
            retrievedOrder.Items.Should().HaveCount(1);
            retrievedOrder.Items[0].ItemId.Should().Be("card789");
            retrievedOrder.Items[0].Quantity.Should().Be(2);
        }

        [TestMethod]
        public async Task GetOrder_NonExistingOrder_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/orders/nonexistent-id");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task CancelOrder_ExistingOrder_ReturnsCanceledOrder()
        {
            // Arrange - Create an order first
            var orderRequest = new OrderCreateRequest
            {
                CustomerId = "customer789",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ItemId = "card123",
                        Quantity = 3,
                        RequestedCondition = ItemCondition.Excellent
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/orders", orderRequest);
            createResponse.EnsureSuccessStatusCode();
            var createdOrder = await createResponse.Content.ReadFromJsonAsync<Order>();
            createdOrder.Should().NotBeNull();
            
            // Act
            var cancelResponse = await _client.PutAsync($"/api/orders/{createdOrder.Id}/cancel", null);
            
            // Assert
            cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var canceledOrder = await cancelResponse.Content.ReadFromJsonAsync<Order>();
            canceledOrder.Should().NotBeNull();
            canceledOrder.Id.Should().Be(createdOrder.Id);
            canceledOrder.Status.Should().Be(OrderStatus.Canceled);
        }
    }
} 