using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TCGOrderManagement.InventoryService.Services;
using TCGOrderManagement.InventoryService.Repositories;
using TCGOrderManagement.InventoryService.Events;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Tests.Services
{
    [TestClass]
    public class InventoryServiceTests
    {
        private Mock<IInventoryRepository> _mockRepository;
        private Mock<IEventPublisher> _mockEventPublisher;
        private InventoryService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IInventoryRepository>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _service = new InventoryService(_mockRepository.Object, _mockEventPublisher.Object);
        }

        [TestMethod]
        public async Task GetInventoryItem_WithValidId_ReturnsItem()
        {
            // Arrange
            var itemId = "card123";
            var condition = ItemCondition.NearMint;
            var expectedItem = new InventoryItem
            {
                Id = itemId,
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = condition,
                QuantityAvailable = 1,
                PricePerUnit = 10000.00m,
                SellerId = "seller456"
            };

            _mockRepository.Setup(r => r.GetInventoryAsync(itemId, condition))
                .ReturnsAsync(expectedItem);

            // Act
            var result = await _service.GetInventoryItemAsync(itemId, condition);

            // Assert
            result.Should().BeEquivalentTo(expectedItem);
            _mockRepository.Verify(r => r.GetInventoryAsync(itemId, condition), Times.Once);
        }

        [TestMethod]
        public async Task ReserveInventory_WithAvailableItems_ReturnsSuccessful()
        {
            // Arrange
            var reservationRequest = new InventoryReservationRequest
            {
                OrderId = "order123",
                Items = new List<ItemRequest>
                {
                    new ItemRequest { ItemId = "card456", Quantity = 2, Condition = ItemCondition.NearMint }
                },
                ExpiryTime = DateTime.UtcNow.AddMinutes(30)
            };

            var inventoryItem = new InventoryItem
            {
                Id = "card456",
                Condition = ItemCondition.NearMint, 
                QuantityAvailable = 5,
                SellerId = "seller789"
            };

            _mockRepository.Setup(r => r.GetInventoryAsync("card456", ItemCondition.NearMint))
                .ReturnsAsync(inventoryItem);

            _mockRepository.Setup(r => r.ReserveInventoryAsync(It.IsAny<InventoryReservation>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReserveInventoryAsync(reservationRequest);

            // Assert
            result.Success.Should().BeTrue();
            result.ReservationId.Should().NotBeNullOrEmpty();
            _mockRepository.Verify(r => r.ReserveInventoryAsync(It.Is<InventoryReservation>(
                res => res.OrderId == reservationRequest.OrderId && 
                      res.ExpiryTime == reservationRequest.ExpiryTime &&
                      res.Items.Count == 1)), 
                Times.Once);
            
            _mockEventPublisher.Verify(p => p.PublishInventoryReservedEventAsync(
                It.Is<InventoryReservedEvent>(e => e.OrderId == reservationRequest.OrderId)), 
                Times.Once);
        }

        [TestMethod]
        public async Task ReserveInventory_WithInsufficientQuantity_ReturnsFailure()
        {
            // Arrange
            var reservationRequest = new InventoryReservationRequest
            {
                OrderId = "order123",
                Items = new List<ItemRequest>
                {
                    new ItemRequest { ItemId = "card456", Quantity = 10, Condition = ItemCondition.NearMint }
                },
                ExpiryTime = DateTime.UtcNow.AddMinutes(30)
            };

            var inventoryItem = new InventoryItem
            {
                Id = "card456",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 5,
                SellerId = "seller789"
            };

            _mockRepository.Setup(r => r.GetInventoryAsync("card456", ItemCondition.NearMint))
                .ReturnsAsync(inventoryItem);

            // Act
            var result = await _service.ReserveInventoryAsync(reservationRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.UnavailableItems.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new UnavailableItem
                {
                    ItemId = "card456",
                    RequestedQuantity = 10,
                    AvailableQuantity = 5,
                    Condition = ItemCondition.NearMint
                });
            
            _mockEventPublisher.Verify(p => p.PublishInventoryReservationFailedEventAsync(
                It.Is<InventoryReservationFailedEvent>(e => 
                    e.OrderId == reservationRequest.OrderId && 
                    e.UnavailableItems.Count == 1)), 
                Times.Once);
        }

        [TestMethod]
        public async Task ConfirmReservation_WithValidId_ReturnsTrue()
        {
            // Arrange
            var reservationId = "reservation123";
            var orderId = "order456";

            _mockRepository.Setup(r => r.ConfirmReservationAsync(reservationId))
                .ReturnsAsync(true);

            _mockRepository.Setup(r => r.GetReservationByIdAsync(reservationId))
                .ReturnsAsync(new InventoryReservation { Id = reservationId, OrderId = orderId });

            // Act
            var result = await _service.ConfirmReservationAsync(reservationId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.ConfirmReservationAsync(reservationId), Times.Once);
            _mockEventPublisher.Verify(p => p.PublishReservationConfirmedEventAsync(
                It.Is<ReservationConfirmedEvent>(e => e.OrderId == orderId)), 
                Times.Once);
        }

        [TestMethod]
        public async Task ReleaseReservation_WithValidId_ReturnsTrue()
        {
            // Arrange
            var reservationId = "reservation123";
            var orderId = "order456";

            _mockRepository.Setup(r => r.ReleaseReservationAsync(reservationId))
                .ReturnsAsync(true);

            _mockRepository.Setup(r => r.GetReservationByIdAsync(reservationId))
                .ReturnsAsync(new InventoryReservation { Id = reservationId, OrderId = orderId });

            // Act
            var result = await _service.ReleaseReservationAsync(reservationId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.ReleaseReservationAsync(reservationId), Times.Once);
            _mockEventPublisher.Verify(p => p.PublishReservationReleasedEventAsync(
                It.Is<ReservationReleasedEvent>(e => e.OrderId == orderId)), 
                Times.Once);
        }
    }
} 