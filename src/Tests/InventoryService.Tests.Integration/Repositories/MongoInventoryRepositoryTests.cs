using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Microsoft.Extensions.Logging.Abstractions;
using TCGOrderManagement.InventoryService.Repositories;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.Shared.Models.Items;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules.Databases;
using DotNet.Testcontainers.Containers.Configurations.Databases;

namespace TCGOrderManagement.InventoryService.Tests.Integration.Repositories
{
    [TestClass]
    public class MongoInventoryRepositoryTests
    {
        private static MongoDbTestcontainer _mongoContainer;
        private MongoClient _mongoClient;
        private MongoInventoryRepository _repository;
        private const string _databaseName = "tcg_inventory_test";

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            _mongoContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
                .WithImage("mongo:latest")
                .WithPortBinding(27017, true)
                .Build();

            await _mongoContainer.StartAsync();
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (_mongoContainer != null)
            {
                await _mongoContainer.StopAsync();
                await _mongoContainer.DisposeAsync();
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _mongoClient = new MongoClient(_mongoContainer.ConnectionString);
            // Clear the database before each test
            _mongoClient.DropDatabase(_databaseName);
            
            _repository = new MongoInventoryRepository(
                _mongoClient, 
                _databaseName,
                new NullLogger<MongoInventoryRepository>());
        }

        [TestMethod]
        public async Task AddInventoryItem_ShouldAddItemToDatabase()
        {
            // Arrange
            var item = new InventoryItem
            {
                Id = "testcard1",
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 1,
                PricePerUnit = 10000.00m,
                SellerId = "seller123"
            };

            // Act
            await _repository.AddInventoryItemAsync(item);
            var retrievedItem = await _repository.GetInventoryAsync("testcard1", ItemCondition.NearMint);

            // Assert
            retrievedItem.Should().NotBeNull();
            retrievedItem.Id.Should().Be(item.Id);
            retrievedItem.Name.Should().Be(item.Name);
            retrievedItem.Condition.Should().Be(item.Condition);
            retrievedItem.QuantityAvailable.Should().Be(item.QuantityAvailable);
        }

        [TestMethod]
        public async Task UpdateInventoryItem_ShouldUpdateItemInDatabase()
        {
            // Arrange
            var item = new InventoryItem
            {
                Id = "testcard1",
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 1,
                PricePerUnit = 10000.00m,
                SellerId = "seller123"
            };

            await _repository.AddInventoryItemAsync(item);

            // Act
            item.QuantityAvailable = 2;
            item.PricePerUnit = 12000.00m;
            await _repository.UpdateInventoryItemAsync(item);
            var retrievedItem = await _repository.GetInventoryAsync("testcard1", ItemCondition.NearMint);

            // Assert
            retrievedItem.Should().NotBeNull();
            retrievedItem.QuantityAvailable.Should().Be(2);
            retrievedItem.PricePerUnit.Should().Be(12000.00m);
        }

        [TestMethod]
        public async Task ReserveInventory_ShouldReduceAvailableQuantity()
        {
            // Arrange
            var item1 = new InventoryItem
            {
                Id = "testcard1",
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 5,
                PricePerUnit = 10000.00m,
                SellerId = "seller123"
            };

            var item2 = new InventoryItem
            {
                Id = "testcard2",
                Name = "Mox Ruby",
                CardSet = "Beta",
                Condition = ItemCondition.Excellent,
                QuantityAvailable = 3,
                PricePerUnit = 5000.00m,
                SellerId = "seller123"
            };

            await _repository.AddInventoryItemAsync(item1);
            await _repository.AddInventoryItemAsync(item2);

            var reservation = new InventoryReservation
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = "order123",
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Items = new List<ReservedItem>
                {
                    new ReservedItem
                    {
                        ItemId = "testcard1",
                        Condition = ItemCondition.NearMint,
                        Quantity = 2,
                        PricePerUnit = 10000.00m
                    },
                    new ReservedItem
                    {
                        ItemId = "testcard2",
                        Condition = ItemCondition.Excellent,
                        Quantity = 1,
                        PricePerUnit = 5000.00m
                    }
                }
            };

            // Act
            var result = await _repository.ReserveInventoryAsync(reservation);
            var item1After = await _repository.GetInventoryAsync("testcard1", ItemCondition.NearMint);
            var item2After = await _repository.GetInventoryAsync("testcard2", ItemCondition.Excellent);

            // Assert
            result.Should().BeTrue();
            item1After.QuantityAvailable.Should().Be(3); // 5 - 2
            item2After.QuantityAvailable.Should().Be(2); // 3 - 1
        }

        [TestMethod]
        public async Task ConfirmReservation_ShouldUpdateReservationStatus()
        {
            // Arrange
            var item = new InventoryItem
            {
                Id = "testcard1",
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 5,
                PricePerUnit = 10000.00m,
                SellerId = "seller123"
            };

            await _repository.AddInventoryItemAsync(item);

            var reservation = new InventoryReservation
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = "order123",
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Items = new List<ReservedItem>
                {
                    new ReservedItem
                    {
                        ItemId = "testcard1",
                        Condition = ItemCondition.NearMint,
                        Quantity = 2,
                        PricePerUnit = 10000.00m
                    }
                }
            };

            await _repository.ReserveInventoryAsync(reservation);

            // Act
            var result = await _repository.ConfirmReservationAsync(reservation.Id);
            var confirmedReservation = await _repository.GetReservationByIdAsync(reservation.Id);

            // Assert
            result.Should().BeTrue();
            confirmedReservation.Should().NotBeNull();
            confirmedReservation.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [TestMethod]
        public async Task ReleaseReservation_ShouldRestoreAvailableQuantity()
        {
            // Arrange
            var item = new InventoryItem
            {
                Id = "testcard1",
                Name = "Black Lotus",
                CardSet = "Alpha",
                Condition = ItemCondition.NearMint,
                QuantityAvailable = 5,
                PricePerUnit = 10000.00m,
                SellerId = "seller123"
            };

            await _repository.AddInventoryItemAsync(item);

            var reservation = new InventoryReservation
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = "order123",
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Items = new List<ReservedItem>
                {
                    new ReservedItem
                    {
                        ItemId = "testcard1",
                        Condition = ItemCondition.NearMint,
                        Quantity = 2,
                        PricePerUnit = 10000.00m
                    }
                }
            };

            await _repository.ReserveInventoryAsync(reservation);
            var itemAfterReserve = await _repository.GetInventoryAsync("testcard1", ItemCondition.NearMint);
            itemAfterReserve.QuantityAvailable.Should().Be(3); // 5 - 2

            // Act
            var result = await _repository.ReleaseReservationAsync(reservation.Id);
            var itemAfterRelease = await _repository.GetInventoryAsync("testcard1", ItemCondition.NearMint);
            var releasedReservation = await _repository.GetReservationByIdAsync(reservation.Id);

            // Assert
            result.Should().BeTrue();
            itemAfterRelease.QuantityAvailable.Should().Be(5); // 3 + 2 (restored)
            releasedReservation.Should().NotBeNull();
            releasedReservation.Status.Should().Be(ReservationStatus.Released);
        }
    }
} 