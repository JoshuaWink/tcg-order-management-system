using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TCGOrderManagement.OrderService.Controllers;
using TCGOrderManagement.OrderService.Services;
using TCGOrderManagement.OrderService.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.OrderService.Tests.Controllers
{
    [TestClass]
    public class OrderControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private OrderController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrderController(_mockOrderService.Object);
        }

        [TestMethod]
        public async Task GetOrder_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = "order123";
            var expectedOrder = new Order 
            { 
                Id = orderId,
                CustomerId = "customer456",
                Items = new List<OrderItem> 
                {
                    new OrderItem 
                    { 
                        ItemId = "card789", 
                        Quantity = 2, 
                        Condition = ItemCondition.NearMint,
                        PricePerUnit = 10.99m
                    }
                },
                Status = OrderStatus.Pending
            };
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            result.Should().BeOfType<ActionResult<Order>>();
            result.Value.Should().BeEquivalentTo(expectedOrder);
            _mockOrderService.Verify(s => s.GetOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var orderId = "nonexistent";
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.GetOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task CreateOrder_WithValidOrder_ReturnsCreatedOrder()
        {
            // Arrange
            var newOrder = new OrderCreateRequest
            {
                CustomerId = "customer456",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ItemId = "card789",
                        Quantity = 2,
                        RequestedCondition = ItemCondition.NearMint
                    }
                }
            };

            var createdOrder = new Order
            {
                Id = "order123",
                CustomerId = newOrder.CustomerId,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ItemId = "card789",
                        Quantity = 2,
                        Condition = ItemCondition.NearMint,
                        PricePerUnit = 10.99m
                    }
                },
                Status = OrderStatus.Created
            };

            _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<OrderCreateRequest>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.CreateOrder(newOrder);

            // Assert
            var createdAtResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.ActionName.Should().Be(nameof(OrderController.GetOrder));
            createdAtResult.RouteValues["id"].Should().Be(createdOrder.Id);
            createdAtResult.Value.Should().BeEquivalentTo(createdOrder);
            
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.Is<OrderCreateRequest>(r => 
                r.CustomerId == newOrder.CustomerId && 
                r.Items.Count == newOrder.Items.Count)), 
                Times.Once);
        }

        [TestMethod]
        public async Task CancelOrder_WithValidId_ReturnsOk()
        {
            // Arrange
            var orderId = "order123";
            var canceledOrder = new Order
            {
                Id = orderId,
                Status = OrderStatus.Canceled
            };

            _mockOrderService.Setup(s => s.CancelOrderAsync(orderId))
                .ReturnsAsync(canceledOrder);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            result.Should().BeOfType<ActionResult<Order>>();
            result.Value.Should().BeEquivalentTo(canceledOrder);
            _mockOrderService.Verify(s => s.CancelOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task CancelOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var orderId = "nonexistent";
            _mockOrderService.Setup(s => s.CancelOrderAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.CancelOrderAsync(orderId), Times.Once);
        }
    }
} 