using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TCGOrderManagement.API.Models.Requests;
using TCGOrderManagement.API.Models.Responses;
using TCGOrderManagement.Shared.Models.Orders;

namespace TCGOrderManagement.API.Tests.Integration.Controllers
{
    [TestClass]
    public class OrderControllerTests
    {
        private static WebApplicationFactorySetup<Program> _factory;
        private static HttpClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _factory = new WebApplicationFactorySetup<Program>();
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [TestMethod]
        public async Task CreateOrder_WithValidRequest_ReturnsCreatedStatusCode()
        {
            // Arrange
            var createOrderRequest = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid().ToString(),
                Items = new[]
                {
                    new OrderItemRequest
                    {
                        ItemId = Guid.NewGuid().ToString(),
                        Quantity = 2,
                        UnitPrice = 10.99m
                    }
                },
                ShippingAddress = new AddressRequest
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "Test Country"
                },
                PaymentMethod = PaymentMethod.CreditCard,
                PaymentDetails = new PaymentDetailsRequest
                {
                    CardNumber = "4111111111111111",
                    ExpirationMonth = 12,
                    ExpirationYear = DateTime.Now.Year + 1,
                    Cvv = "123",
                    CardholderName = "Test User"
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", createOrderRequest);
            var responseContent = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            responseContent.Should().NotBeNull();
            responseContent.OrderId.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task GetOrder_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = "test-order-id"; // This should be set up in the mock

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
            orderResponse.Should().NotBeNull();
            orderResponse.Id.Should().Be(orderId);
        }

        [TestMethod]
        public async Task GetOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidOrderId = "non-existent-order-id";

            // Act
            var response = await _client.GetAsync($"/api/orders/{invalidOrderId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateOrderStatus_WithValidRequest_ReturnsOkStatusCode()
        {
            // Arrange
            var orderId = "test-order-id"; // This should be set up in the mock
            var updateStatusRequest = new UpdateOrderStatusRequest
            {
                Status = OrderStatus.Processing
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateStatusRequest);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task CancelOrder_WithValidId_ReturnsOkStatusCode()
        {
            // Arrange
            var orderId = "test-order-id"; // This should be set up in the mock

            // Act
            var response = await _client.DeleteAsync($"/api/orders/{orderId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
} 