using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TCGOrderManagement.InventoryService.Configuration;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Repositories
{
    /// <summary>
    /// MongoDB implementation of the inventory repository
    /// </summary>
    public class MongoInventoryRepository : IInventoryRepository
    {
        private readonly IMongoCollection<InventoryItem> _itemsCollection;
        private readonly IMongoCollection<InventoryReservation> _reservationsCollection;
        private readonly ILogger<MongoInventoryRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the MongoInventoryRepository class
        /// </summary>
        /// <param name="database">The MongoDB database</param>
        /// <param name="settings">The MongoDB settings</param>
        /// <param name="logger">The logger</param>
        public MongoInventoryRepository(
            IMongoDatabase database, 
            MongoDbSettings settings,
            ILogger<MongoInventoryRepository> logger)
        {
            _logger = logger;

            // Initialize collections
            _itemsCollection = database.GetCollection<InventoryItem>(settings.InventoryCollectionName);
            _reservationsCollection = database.GetCollection<InventoryReservation>(settings.ReservationsCollectionName);

            // Create indexes
            CreateIndexes();
        }

        /// <summary>
        /// Creates indexes for performance optimization
        /// </summary>
        private void CreateIndexes()
        {
            try
            {
                // Create indexes for inventory items
                var itemIndexes = new List<CreateIndexModel<InventoryItem>>
                {
                    new CreateIndexModel<InventoryItem>(
                        Builders<InventoryItem>.IndexKeys.Ascending(i => i.SellerId),
                        new CreateIndexOptions { Background = true }),
                    new CreateIndexModel<InventoryItem>(
                        Builders<InventoryItem>.IndexKeys.Text(i => i.CardName)
                            .Text(i => i.SetName),
                        new CreateIndexOptions { Background = true }),
                    new CreateIndexModel<InventoryItem>(
                        Builders<InventoryItem>.IndexKeys
                            .Ascending(i => i.SetCode)
                            .Ascending(i => i.CollectorNumber),
                        new CreateIndexOptions { Background = true })
                };

                _itemsCollection.Indexes.CreateMany(itemIndexes);

                // Create indexes for reservations
                var reservationIndexes = new List<CreateIndexModel<InventoryReservation>>
                {
                    new CreateIndexModel<InventoryReservation>(
                        Builders<InventoryReservation>.IndexKeys.Ascending(r => r.OrderId),
                        new CreateIndexOptions { Background = true, Unique = true }),
                    new CreateIndexModel<InventoryReservation>(
                        Builders<InventoryReservation>.IndexKeys.Ascending(r => r.ExpirationTime),
                        new CreateIndexOptions { Background = true }),
                    new CreateIndexModel<InventoryReservation>(
                        Builders<InventoryReservation>.IndexKeys.Ascending(r => r.Status),
                        new CreateIndexOptions { Background = true })
                };

                _reservationsCollection.Indexes.CreateMany(reservationIndexes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MongoDB indexes");
                // Don't throw - the application can still function without optimal indexes
            }
        }

        /// <summary>
        /// Gets inventory items based on specified criteria
        /// </summary>
        /// <param name="sellerId">Optional seller ID filter</param>
        /// <param name="itemIds">Optional list of specific item IDs to retrieve</param>
        /// <param name="cardName">Optional card name filter</param>
        /// <param name="setName">Optional set name filter</param>
        /// <param name="minCondition">Optional minimum condition filter</param>
        /// <returns>A collection of inventory items matching the criteria</returns>
        public async Task<IEnumerable<InventoryItem>> GetInventoryAsync(
            Guid? sellerId = null,
            IEnumerable<Guid> itemIds = null,
            string cardName = null,
            string setName = null,
            ItemCondition? minCondition = null)
        {
            try
            {
                var filter = Builders<InventoryItem>.Filter.Empty;

                // Apply filters
                if (sellerId.HasValue)
                {
                    filter &= Builders<InventoryItem>.Filter.Eq(i => i.SellerId, sellerId.Value);
                }

                if (itemIds != null && itemIds.Any())
                {
                    filter &= Builders<InventoryItem>.Filter.In(i => i.Id, itemIds);
                }

                if (!string.IsNullOrWhiteSpace(cardName))
                {
                    filter &= Builders<InventoryItem>.Filter.Regex(i => i.CardName, 
                        new BsonRegularExpression(cardName, "i"));
                }

                if (!string.IsNullOrWhiteSpace(setName))
                {
                    filter &= Builders<InventoryItem>.Filter.Regex(i => i.SetName, 
                        new BsonRegularExpression(setName, "i"));
                }

                if (minCondition.HasValue)
                {
                    filter &= Builders<InventoryItem>.Filter.Gte(i => i.Condition, minCondition.Value);
                }

                // Only return items with available quantity
                filter &= Builders<InventoryItem>.Filter.Gt(i => i.AvailableQuantity, 0);

                var items = await _itemsCollection.Find(filter)
                    .Sort(Builders<InventoryItem>.Sort.Descending(i => i.LastUpdated))
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory items");
                throw;
            }
        }

        /// <summary>
        /// Adds a new inventory item
        /// </summary>
        /// <param name="item">The inventory item to add</param>
        /// <returns>The added inventory item with generated ID</returns>
        public async Task<InventoryItem> AddInventoryItemAsync(InventoryItem item)
        {
            try
            {
                // Set creation and update timestamps
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                }

                var now = DateTime.UtcNow;
                item.ListedDate = now;
                item.LastUpdated = now;
                item.ReservedQuantity = 0; // Ensure new items start with 0 reserved

                await _itemsCollection.InsertOneAsync(item);
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory item");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing inventory item
        /// </summary>
        /// <param name="item">The inventory item to update</param>
        /// <returns>True if update was successful</returns>
        public async Task<bool> UpdateInventoryItemAsync(InventoryItem item)
        {
            try
            {
                // Update the last updated timestamp
                item.LastUpdated = DateTime.UtcNow;

                var filter = Builders<InventoryItem>.Filter.Eq(i => i.Id, item.Id);
                var update = Builders<InventoryItem>.Update
                    .Set(i => i.CardName, item.CardName)
                    .Set(i => i.SetName, item.SetName)
                    .Set(i => i.SetCode, item.SetCode)
                    .Set(i => i.CollectorNumber, item.CollectorNumber)
                    .Set(i => i.Rarity, item.Rarity)
                    .Set(i => i.Condition, item.Condition)
                    .Set(i => i.IsFoil, item.IsFoil)
                    .Set(i => i.Language, item.Language)
                    .Set(i => i.SellerNotes, item.SellerNotes)
                    .Set(i => i.PriceCents, item.PriceCents)
                    .Set(i => i.AvailableQuantity, item.AvailableQuantity)
                    .Set(i => i.ImageUrl, item.ImageUrl)
                    .Set(i => i.LastUpdated, item.LastUpdated);

                var result = await _itemsCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item {ItemId}", item.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes an inventory item by ID
        /// </summary>
        /// <param name="itemId">The ID of the item to delete</param>
        /// <returns>True if deletion was successful</returns>
        public async Task<bool> DeleteInventoryItemAsync(Guid itemId)
        {
            try
            {
                // First check if the item has any active reservations
                var reservations = await _reservationsCollection
                    .Find(r => r.Status == ReservationStatus.Active && r.Items.Any(i => i.ItemId == itemId))
                    .FirstOrDefaultAsync();

                if (reservations != null)
                {
                    _logger.LogWarning("Cannot delete inventory item {ItemId} as it has active reservations", itemId);
                    return false;
                }

                var result = await _itemsCollection.DeleteOneAsync(i => i.Id == itemId);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item {ItemId}", itemId);
                throw;
            }
        }

        /// <summary>
        /// Verifies if the specified seller owns the specified item
        /// </summary>
        /// <param name="itemId">Item ID to check</param>
        /// <param name="sellerId">Seller ID to verify against</param>
        /// <returns>True if the seller owns the item</returns>
        public async Task<bool> SellerOwnsItemAsync(Guid itemId, Guid sellerId)
        {
            try
            {
                var filter = Builders<InventoryItem>.Filter.Eq(i => i.Id, itemId) &
                             Builders<InventoryItem>.Filter.Eq(i => i.SellerId, sellerId);

                var count = await _itemsCollection.CountDocumentsAsync(filter);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if seller {SellerId} owns item {ItemId}", sellerId, itemId);
                throw;
            }
        }

        /// <summary>
        /// Reserves inventory items for an order
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="userId">The user ID making the reservation</param>
        /// <param name="items">The items to reserve with quantities</param>
        /// <param name="expirationTime">When the reservation expires</param>
        /// <returns>Result of the reservation attempt with any unavailable items</returns>
        public async Task<(bool Success, IEnumerable<UnavailableItem> UnavailableItems)> ReserveInventoryAsync(
            Guid orderId,
            Guid userId,
            IEnumerable<(Guid ItemId, int Quantity)> items,
            DateTime expirationTime)
        {
            // Use a client-side transaction to ensure atomicity
            var unavailableItems = new List<UnavailableItem>();
            
            // Start a session
            using var session = await _itemsCollection.Database.Client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                // Check existing reservation
                var existingReservation = await _reservationsCollection
                    .Find(session, r => r.OrderId == orderId)
                    .FirstOrDefaultAsync();
                
                if (existingReservation != null)
                {
                    // Cannot create a new reservation if one already exists
                    await session.AbortTransactionAsync();
                    _logger.LogWarning("Reservation already exists for order {OrderId}", orderId);
                    return (false, Array.Empty<UnavailableItem>());
                }
                
                // Get all requested items to check availability
                var itemList = items.ToList();
                var itemIds = itemList.Select(i => i.ItemId).ToList();
                var inventoryItems = await _itemsCollection
                    .Find(session, item => itemIds.Contains(item.Id))
                    .ToListAsync();
                
                // Check if all items are available in requested quantities
                var reservedItems = new List<ReservedItem>();
                
                foreach (var (itemId, quantity) in itemList)
                {
                    var inventoryItem = inventoryItems.FirstOrDefault(i => i.Id == itemId);
                    
                    if (inventoryItem == null)
                    {
                        unavailableItems.Add(new UnavailableItem(itemId, "Item not found", quantity, 0));
                        continue;
                    }
                    
                    var availableQty = inventoryItem.AvailableQuantity - inventoryItem.ReservedQuantity;
                    
                    if (availableQty < quantity)
                    {
                        unavailableItems.Add(new UnavailableItem(
                            itemId, 
                            inventoryItem.CardName, 
                            quantity, 
                            availableQty));
                        continue;
                    }
                    
                    reservedItems.Add(new ReservedItem
                    {
                        ItemId = itemId,
                        Quantity = quantity,
                        PriceCents = inventoryItem.PriceCents,
                        ItemName = inventoryItem.CardName
                    });
                    
                    // Update the reserved quantity in the inventory item
                    var filter = Builders<InventoryItem>.Filter.Eq(i => i.Id, itemId);
                    var update = Builders<InventoryItem>.Update
                        .Inc(i => i.ReservedQuantity, quantity)
                        .Set(i => i.LastUpdated, DateTime.UtcNow);
                    
                    await _itemsCollection.UpdateOneAsync(session, filter, update);
                }
                
                // If there are unavailable items, abort and return them
                if (unavailableItems.Any())
                {
                    await session.AbortTransactionAsync();
                    return (false, unavailableItems);
                }
                
                // Create the reservation
                var reservation = new InventoryReservation
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    ExpirationTime = expirationTime,
                    Status = ReservationStatus.Active,
                    Items = reservedItems
                };
                
                await _reservationsCollection.InsertOneAsync(session, reservation);
                
                // Commit the transaction
                await session.CommitTransactionAsync();
                
                return (true, Array.Empty<UnavailableItem>());
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error reserving inventory for order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Confirms a reservation, permanently removing items from inventory
        /// </summary>
        /// <param name="orderId">The order ID associated with the reservation</param>
        /// <returns>True if confirmation was successful</returns>
        public async Task<bool> ConfirmReservationAsync(Guid orderId)
        {
            using var session = await _itemsCollection.Database.Client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                // Get the reservation
                var reservation = await _reservationsCollection
                    .Find(session, r => r.OrderId == orderId && r.Status == ReservationStatus.Active)
                    .FirstOrDefaultAsync();
                
                if (reservation == null)
                {
                    _logger.LogWarning("No active reservation found for order {OrderId}", orderId);
                    await session.AbortTransactionAsync();
                    return false;
                }
                
                // Update the reservation status
                var reservationFilter = Builders<InventoryReservation>.Filter.Eq(r => r.Id, reservation.Id);
                var reservationUpdate = Builders<InventoryReservation>.Update
                    .Set(r => r.Status, ReservationStatus.Confirmed)
                    .Set(r => r.ConfirmedAt, DateTime.UtcNow);
                
                await _reservationsCollection.UpdateOneAsync(session, reservationFilter, reservationUpdate);
                
                // Update the inventory items by reducing available quantity and clearing reserved quantity
                foreach (var item in reservation.Items)
                {
                    var itemFilter = Builders<InventoryItem>.Filter.Eq(i => i.Id, item.ItemId);
                    var itemUpdate = Builders<InventoryItem>.Update
                        .Inc(i => i.AvailableQuantity, -item.Quantity)
                        .Inc(i => i.ReservedQuantity, -item.Quantity)
                        .Set(i => i.LastUpdated, DateTime.UtcNow);
                    
                    await _itemsCollection.UpdateOneAsync(session, itemFilter, itemUpdate);
                }
                
                await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error confirming reservation for order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Releases a reservation, returning items to available inventory
        /// </summary>
        /// <param name="orderId">The order ID associated with the reservation</param>
        /// <returns>True if release was successful</returns>
        public async Task<bool> ReleaseReservationAsync(Guid orderId)
        {
            using var session = await _itemsCollection.Database.Client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                // Get the reservation
                var reservation = await _reservationsCollection
                    .Find(session, r => r.OrderId == orderId && r.Status == ReservationStatus.Active)
                    .FirstOrDefaultAsync();
                
                if (reservation == null)
                {
                    _logger.LogWarning("No active reservation found to release for order {OrderId}", orderId);
                    await session.AbortTransactionAsync();
                    return false;
                }
                
                // Update the reservation status
                var reservationFilter = Builders<InventoryReservation>.Filter.Eq(r => r.Id, reservation.Id);
                var reservationUpdate = Builders<InventoryReservation>.Update
                    .Set(r => r.Status, ReservationStatus.Released)
                    .Set(r => r.ReleasedAt, DateTime.UtcNow);
                
                await _reservationsCollection.UpdateOneAsync(session, reservationFilter, reservationUpdate);
                
                // Update the inventory items by reducing reserved quantity
                foreach (var item in reservation.Items)
                {
                    var itemFilter = Builders<InventoryItem>.Filter.Eq(i => i.Id, item.ItemId);
                    var itemUpdate = Builders<InventoryItem>.Update
                        .Inc(i => i.ReservedQuantity, -item.Quantity)
                        .Set(i => i.LastUpdated, DateTime.UtcNow);
                    
                    await _itemsCollection.UpdateOneAsync(session, itemFilter, itemUpdate);
                }
                
                await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error releasing reservation for order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Cleans up expired reservations, returning items to available inventory
        /// </summary>
        /// <returns>Number of expired reservations cleaned up</returns>
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            using var session = await _itemsCollection.Database.Client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                // Find all expired active reservations
                var now = DateTime.UtcNow;
                var expiredReservations = await _reservationsCollection
                    .Find(session, r => r.Status == ReservationStatus.Active && r.ExpirationTime < now)
                    .ToListAsync();
                
                if (!expiredReservations.Any())
                {
                    await session.AbortTransactionAsync();
                    return 0;
                }
                
                // Update each reservation and its associated inventory items
                foreach (var reservation in expiredReservations)
                {
                    // Update the reservation status
                    var reservationFilter = Builders<InventoryReservation>.Filter.Eq(r => r.Id, reservation.Id);
                    var reservationUpdate = Builders<InventoryReservation>.Update
                        .Set(r => r.Status, ReservationStatus.Expired)
                        .Set(r => r.ReleasedAt, now);
                    
                    await _reservationsCollection.UpdateOneAsync(session, reservationFilter, reservationUpdate);
                    
                    // Update the inventory items by reducing reserved quantity
                    foreach (var item in reservation.Items)
                    {
                        var itemFilter = Builders<InventoryItem>.Filter.Eq(i => i.Id, item.ItemId);
                        var itemUpdate = Builders<InventoryItem>.Update
                            .Inc(i => i.ReservedQuantity, -item.Quantity)
                            .Set(i => i.LastUpdated, now);
                        
                        await _itemsCollection.UpdateOneAsync(session, itemFilter, itemUpdate);
                    }
                }
                
                await session.CommitTransactionAsync();
                
                _logger.LogInformation("Cleaned up {Count} expired reservations", expiredReservations.Count);
                return expiredReservations.Count;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error cleaning up expired reservations");
                throw;
            }
        }
    }

    /// <summary>
    /// Represents a reservation of inventory items
    /// </summary>
    public class InventoryReservation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reservation
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the order associated with this reservation
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who made the reservation
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the reservation was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the reservation expires
        /// </summary>
        public DateTime ExpirationTime { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the reservation was confirmed
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the reservation was released
        /// </summary>
        public DateTime? ReleasedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the status of the reservation
        /// </summary>
        public ReservationStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the items included in this reservation
        /// </summary>
        public List<ReservedItem> Items { get; set; } = new List<ReservedItem>();
    }

    /// <summary>
    /// Represents an item that has been reserved
    /// </summary>
    public class ReservedItem
    {
        /// <summary>
        /// Gets or sets the ID of the inventory item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the item
        /// </summary>
        public string ItemName { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity reserved
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the price of the item at the time of reservation (in cents)
        /// </summary>
        public int PriceCents { get; set; }
    }

    /// <summary>
    /// Represents the status of an inventory reservation
    /// </summary>
    public enum ReservationStatus
    {
        /// <summary>
        /// Reservation is active and items are reserved
        /// </summary>
        Active = 0,
        
        /// <summary>
        /// Reservation has been confirmed and items have been permanently removed from inventory
        /// </summary>
        Confirmed = 1,
        
        /// <summary>
        /// Reservation has been released and items have been returned to available inventory
        /// </summary>
        Released = 2,
        
        /// <summary>
        /// Reservation has expired and items have been returned to available inventory
        /// </summary>
        Expired = 3
    }
} 