using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.InventoryService.Repositories;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.API.Tests.Integration.Mocks
{
    public class MockInventoryRepository : IInventoryRepository
    {
        private readonly List<InventoryItem> _inventoryItems;
        private readonly List<InventoryReservation> _reservations;

        public MockInventoryRepository()
        {
            // Initialize with some test data
            _inventoryItems = new List<InventoryItem>
            {
                new InventoryItem
                {
                    Id = "item1",
                    Name = "Black Lotus",
                    Description = "Rare Magic the Gathering card",
                    Condition = ItemCondition.NearMint,
                    Price = 10000.00m,
                    Quantity = 1,
                    SellerId = "seller1",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                },
                new InventoryItem
                {
                    Id = "item2",
                    Name = "Charizard 1st Edition",
                    Description = "Rare Pokemon card",
                    Condition = ItemCondition.Mint,
                    Price = 5000.00m,
                    Quantity = 2,
                    SellerId = "seller1",
                    CreatedAt = DateTime.Now.AddDays(-20),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                },
                new InventoryItem
                {
                    Id = "item3",
                    Name = "Blue-Eyes White Dragon",
                    Description = "Rare Yu-Gi-Oh card",
                    Condition = ItemCondition.Excellent,
                    Price = 500.00m,
                    Quantity = 3,
                    SellerId = "seller2",
                    CreatedAt = DateTime.Now.AddDays(-15),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                }
            };

            _reservations = new List<InventoryReservation>();
        }

        public Task<InventoryItem> AddInventoryItemAsync(InventoryItem item)
        {
            item.Id = Guid.NewGuid().ToString();
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
            _inventoryItems.Add(item);
            return Task.FromResult(item);
        }

        public Task CleanupExpiredReservationsAsync()
        {
            _reservations.RemoveAll(r => r.ExpiresAt < DateTime.Now);
            return Task.CompletedTask;
        }

        public Task ConfirmReservationAsync(string reservationId)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation != null)
            {
                reservation.Status = ReservationStatus.Confirmed;
                
                // Update quantities for confirmed items
                foreach (var item in reservation.Items)
                {
                    var inventoryItem = _inventoryItems.FirstOrDefault(i => i.Id == item.ItemId);
                    if (inventoryItem != null)
                    {
                        inventoryItem.Quantity -= item.Quantity;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task<bool> DeleteInventoryItemAsync(string id)
        {
            var item = _inventoryItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _inventoryItems.Remove(item);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IEnumerable<InventoryItem>> GetInventoryAsync(
            string sellerId = null, 
            string searchQuery = null,
            int page = 1, 
            int pageSize = 20)
        {
            var query = _inventoryItems.AsQueryable();
            
            if (!string.IsNullOrEmpty(sellerId))
            {
                query = query.Where(i => i.SellerId == sellerId);
            }
            
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(i => 
                    i.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || 
                    i.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }
            
            return Task.FromResult(query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable());
        }

        public Task<InventoryItem> GetInventoryItemAsync(string id)
        {
            var item = _inventoryItems.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item);
        }

        public Task ReleaseReservationAsync(string reservationId)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation != null)
            {
                _reservations.Remove(reservation);
            }
            return Task.CompletedTask;
        }

        public Task<InventoryReservation> ReserveInventoryAsync(
            string orderId, 
            IEnumerable<KeyValuePair<string, int>> itemsToReserve, 
            TimeSpan reservationDuration)
        {
            var reservedItems = new List<ReservedItem>();
            var unavailableItems = new List<UnavailableItem>();
            
            foreach (var itemToReserve in itemsToReserve)
            {
                var inventoryItem = _inventoryItems.FirstOrDefault(i => i.Id == itemToReserve.Key);
                if (inventoryItem == null || inventoryItem.Quantity < itemToReserve.Value)
                {
                    unavailableItems.Add(new UnavailableItem
                    {
                        ItemId = itemToReserve.Key,
                        RequestedQuantity = itemToReserve.Value,
                        AvailableQuantity = inventoryItem?.Quantity ?? 0
                    });
                }
                else
                {
                    reservedItems.Add(new ReservedItem
                    {
                        ItemId = itemToReserve.Key,
                        Quantity = itemToReserve.Value,
                        Price = inventoryItem.Price
                    });
                }
            }
            
            if (unavailableItems.Any())
            {
                throw new InvalidOperationException("Some items are not available in the requested quantity");
            }
            
            var reservation = new InventoryReservation
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = orderId,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.Add(reservationDuration),
                Items = reservedItems
            };
            
            _reservations.Add(reservation);
            return Task.FromResult(reservation);
        }

        public Task<bool> SellerOwnsItemAsync(string sellerId, string itemId)
        {
            var item = _inventoryItems.FirstOrDefault(i => i.Id == itemId);
            return Task.FromResult(item != null && item.SellerId == sellerId);
        }

        public Task<InventoryItem> UpdateInventoryItemAsync(InventoryItem item)
        {
            var existingItem = _inventoryItems.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.Description = item.Description;
                existingItem.Condition = item.Condition;
                existingItem.Price = item.Price;
                existingItem.Quantity = item.Quantity;
                existingItem.UpdatedAt = DateTime.Now;
                return Task.FromResult(existingItem);
            }
            return Task.FromResult<InventoryItem>(null);
        }
    }
} 