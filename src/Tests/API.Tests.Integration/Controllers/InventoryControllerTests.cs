using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TCGOrderManagement.API.Models.Requests;
using TCGOrderManagement.API.Models.Responses;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.API.Tests.Integration.Controllers
{
    [TestClass]
    public class InventoryControllerTests
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
        public async Task GetInventoryItems_ReturnsSuccessStatusCodeAndItems()
        {
            // Act
            var response = await _client.GetAsync("/api/inventory");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var items = await response.Content.ReadFromJsonAsync<InventoryItemResponse[]>();
            items.Should().NotBeNull();
            items.Length.Should().BeGreaterOrEqual(0);
        }

        [TestMethod]
        public async Task GetInventoryItem_WithValidId_ReturnsItem()
        {
            // Arrange
            var itemId = "test-item-id"; // Should be configured in the mock
            
            // Act
            var response = await _client.GetAsync($"/api/inventory/{itemId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var item = await response.Content.ReadFromJsonAsync<InventoryItemResponse>();
            item.Should().NotBeNull();
            item.Id.Should().Be(itemId);
        }

        [TestMethod]
        public async Task GetInventoryItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidItemId = "non-existent-item-id";
            
            // Act
            var response = await _client.GetAsync($"/api/inventory/{invalidItemId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task CreateInventoryItem_WithValidRequest_ReturnsCreatedStatusCode()
        {
            // Arrange
            var createItemRequest = new CreateInventoryItemRequest
            {
                SellerId = Guid.NewGuid().ToString(),
                Name = "Test Card",
                Description = "A test trading card",
                Category = "Trading Cards",
                Condition = ItemCondition.NearMint,
                Price = 15.99m,
                Quantity = 5,
                ImageUrls = new[] { "https://example.com/image1.jpg" }
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/inventory", createItemRequest);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadFromJsonAsync<CreateInventoryItemResponse>();
            responseContent.Should().NotBeNull();
            responseContent.ItemId.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task UpdateInventoryItem_WithValidRequest_ReturnsOkStatusCode()
        {
            // Arrange
            var itemId = "test-item-id"; // Should be configured in the mock
            var updateItemRequest = new UpdateInventoryItemRequest
            {
                Name = "Updated Card Name",
                Description = "Updated description",
                Price = 19.99m,
                Quantity = 10,
                Condition = ItemCondition.Mint
            };
            
            // Act
            var response = await _client.PutAsJsonAsync($"/api/inventory/{itemId}", updateItemRequest);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task DeleteInventoryItem_WithValidId_ReturnsOkStatusCode()
        {
            // Arrange
            var itemId = "test-item-id"; // Should be configured in the mock
            
            // Act
            var response = await _client.DeleteAsync($"/api/inventory/{itemId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
} 