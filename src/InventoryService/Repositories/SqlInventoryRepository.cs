using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using TCGOrderManagement.InventoryService.Models;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Repositories
{
    /// <summary>
    /// SQL Server implementation of the inventory repository
    /// </summary>
    public class SqlInventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;
        private readonly int _defaultReservationExpiryMinutes;

        /// <summary>
        /// Initializes a new instance of the SqlInventoryRepository class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public SqlInventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("InventoryDatabase");
            _defaultReservationExpiryMinutes = configuration.GetValue<int>("InventorySettings:ReservationExpiryMinutes", 15);
        }

        /// <inheritdoc />
        public async Task<InventoryPagedResult> GetInventoryAsync(int page, int pageSize, string category = null, string searchTerm = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var whereClause = BuildWhereClause(category, searchTerm);
                
                // Get total count
                var countSql = $"SELECT COUNT(*) FROM Inventory {whereClause}";
                var totalItems = await connection.ExecuteScalarAsync<int>(countSql);
                
                // Get items for current page
                var pageSql = $@"
                    SELECT i.*, s.Name AS SellerName
                    FROM Inventory i
                    LEFT JOIN Sellers s ON i.SellerId = s.Id
                    {whereClause}
                    ORDER BY i.Name
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                var parameters = new
                {
                    Category = category,
                    SearchTerm = searchTerm != null ? $"%{searchTerm}%" : null,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                };
                
                var items = await connection.QueryAsync<InventoryItem>(pageSql, parameters);
                
                return new InventoryPagedResult
                {
                    Items = items.ToList(),
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
        }

        /// <inheritdoc />
        public async Task<InventoryItem> GetItemByIdAsync(Guid itemId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT i.*, s.Name AS SellerName
                    FROM Inventory i
                    LEFT JOIN Sellers s ON i.SellerId = s.Id
                    WHERE i.Id = @ItemId";
                
                return await connection.QueryFirstOrDefaultAsync<InventoryItem>(sql, new { ItemId = itemId });
            }
        }

        /// <inheritdoc />
        public async Task<InventoryOperationResult> AddInventoryItemAsync(InventoryItem item)
        {
            if (item == null)
                return InventoryOperationResult.Failure("Item cannot be null");
                
            if (string.IsNullOrWhiteSpace(item.Name))
                return InventoryOperationResult.Failure("Item name is required");
                
            if (item.Price <= 0)
                return InventoryOperationResult.Failure("Item price must be greater than zero");
                
            if (item.AvailableQuantity < 0)
                return InventoryOperationResult.Failure("Available quantity cannot be negative");
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Ensure item ID is set
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                }
                
                var sql = @"
                    INSERT INTO Inventory (
                        Id, Name, Description, Category, Price, AvailableQuantity, 
                        Condition, ImageUrl, SellerId, DateAdded, LastUpdated
                    ) VALUES (
                        @Id, @Name, @Description, @Category, @Price, @AvailableQuantity, 
                        @Condition, @ImageUrl, @SellerId, @DateAdded, @LastUpdated
                    )";
                
                try
                {
                    var now = DateTime.UtcNow;
                    item.DateAdded = now;
                    item.LastUpdated = now;
                    
                    await connection.ExecuteAsync(sql, item);
                    return InventoryOperationResult.Success("Item added successfully", item.Id);
                }
                catch (Exception ex)
                {
                    return InventoryOperationResult.Failure($"Failed to add item: {ex.Message}");
                }
            }
        }

        /// <inheritdoc />
        public async Task<InventoryOperationResult> UpdateInventoryItemAsync(Guid itemId, InventoryItem item)
        {
            if (item == null)
                return InventoryOperationResult.Failure("Item cannot be null");
                
            if (string.IsNullOrWhiteSpace(item.Name))
                return InventoryOperationResult.Failure("Item name is required");
                
            if (item.Price <= 0)
                return InventoryOperationResult.Failure("Item price must be greater than zero");
                
            if (item.AvailableQuantity < 0)
                return InventoryOperationResult.Failure("Available quantity cannot be negative");
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Check if item exists
                var existingItem = await GetItemByIdAsync(itemId);
                if (existingItem == null)
                {
                    return InventoryOperationResult.Failure($"Item with ID {itemId} not found");
                }
                
                // Check if seller owns the item
                if (!await SellerOwnsItemAsync(itemId, item.SellerId))
                {
                    return InventoryOperationResult.Failure("You don't have permission to update this item");
                }
                
                var sql = @"
                    UPDATE Inventory SET
                        Name = @Name,
                        Description = @Description,
                        Category = @Category,
                        Price = @Price,
                        AvailableQuantity = @AvailableQuantity,
                        Condition = @Condition,
                        ImageUrl = @ImageUrl,
                        LastUpdated = @LastUpdated
                    WHERE Id = @Id";
                
                try
                {
                    // Preserve original ID and seller
                    item.Id = itemId;
                    item.SellerId = existingItem.SellerId;
                    item.DateAdded = existingItem.DateAdded;
                    item.LastUpdated = DateTime.UtcNow;
                    
                    await connection.ExecuteAsync(sql, item);
                    return InventoryOperationResult.Success("Item updated successfully", itemId);
                }
                catch (Exception ex)
                {
                    return InventoryOperationResult.Failure($"Failed to update item: {ex.Message}");
                }
            }
        }

        /// <inheritdoc />
        public async Task<InventoryOperationResult> DeleteInventoryItemAsync(Guid itemId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Check if item exists
                var existingItem = await GetItemByIdAsync(itemId);
                if (existingItem == null)
                {
                    return InventoryOperationResult.Failure($"Item with ID {itemId} not found");
                }
                
                // Check for active reservations
                var reservationSql = "SELECT COUNT(*) FROM Reservations WHERE ItemId = @ItemId AND IsActive = 1";
                var reservationCount = await connection.ExecuteScalarAsync<int>(reservationSql, new { ItemId = itemId });
                
                if (reservationCount > 0)
                {
                    return InventoryOperationResult.Failure("Item cannot be deleted as it has active reservations");
                }
                
                var sql = "DELETE FROM Inventory WHERE Id = @ItemId";
                
                try
                {
                    await connection.ExecuteAsync(sql, new { ItemId = itemId });
                    return InventoryOperationResult.Success("Item deleted successfully", itemId);
                }
                catch (Exception ex)
                {
                    return InventoryOperationResult.Failure($"Failed to delete item: {ex.Message}");
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> SellerOwnsItemAsync(Guid itemId, Guid sellerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "SELECT COUNT(*) FROM Inventory WHERE Id = @ItemId AND SellerId = @SellerId";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { ItemId = itemId, SellerId = sellerId });
                
                return count > 0;
            }
        }

        /// <inheritdoc />
        public async Task<InventoryReservationResult> ReserveInventoryAsync(List<OrderItemRequest> items, Guid orderId, int expiryMinutes)
        {
            if (items == null || !items.Any())
                return InventoryReservationResult.Failure("No items to reserve");
                
            if (orderId == Guid.Empty)
                return InventoryReservationResult.Failure("Order ID is required");
                
            // Use default expiry if not specified or invalid
            if (expiryMinutes <= 0)
                expiryMinutes = _defaultReservationExpiryMinutes;
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Start transaction
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var unavailableItems = new List<UnavailableItem>();
                        
                        // Check and reserve each item
                        foreach (var item in items)
                        {
                            // Get current inventory
                            var inventorySql = "SELECT Id, AvailableQuantity FROM Inventory WHERE Id = @ItemId";
                            var inventoryItem = await connection.QueryFirstOrDefaultAsync<InventoryItem>(
                                inventorySql, 
                                new { ItemId = item.ItemId },
                                transaction);
                                
                            if (inventoryItem == null)
                            {
                                unavailableItems.Add(new UnavailableItem
                                {
                                    ItemId = item.ItemId,
                                    RequestedQuantity = item.Quantity,
                                    AvailableQuantity = 0
                                });
                                continue;
                            }
                            
                            if (inventoryItem.AvailableQuantity < item.Quantity)
                            {
                                unavailableItems.Add(new UnavailableItem
                                {
                                    ItemId = item.ItemId,
                                    RequestedQuantity = item.Quantity,
                                    AvailableQuantity = inventoryItem.AvailableQuantity
                                });
                                continue;
                            }
                            
                            // Update inventory
                            var updateSql = @"
                                UPDATE Inventory 
                                SET AvailableQuantity = AvailableQuantity - @Quantity 
                                WHERE Id = @ItemId";
                                
                            await connection.ExecuteAsync(
                                updateSql, 
                                new { ItemId = item.ItemId, Quantity = item.Quantity },
                                transaction);
                                
                            // Create reservation
                            var expiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);
                            var reservationSql = @"
                                INSERT INTO Reservations (
                                    Id, OrderId, ItemId, Quantity, ExpiryDate, IsActive
                                ) VALUES (
                                    @Id, @OrderId, @ItemId, @Quantity, @ExpiryDate, 1
                                )";
                                
                            await connection.ExecuteAsync(
                                reservationSql,
                                new 
                                {
                                    Id = Guid.NewGuid(),
                                    OrderId = orderId,
                                    ItemId = item.ItemId,
                                    Quantity = item.Quantity,
                                    ExpiryDate = expiryDate
                                },
                                transaction);
                        }
                        
                        // If any items are unavailable, rollback and return failure
                        if (unavailableItems.Any())
                        {
                            await transaction.RollbackAsync();
                            return InventoryReservationResult.Failure(
                                "Some items are unavailable or have insufficient quantity",
                                unavailableItems);
                        }
                        
                        // Commit transaction
                        await transaction.CommitAsync();
                        
                        return InventoryReservationResult.Success(
                            $"Inventory reserved successfully until {DateTime.UtcNow.AddMinutes(expiryMinutes):yyyy-MM-dd HH:mm:ss} UTC");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return InventoryReservationResult.Failure($"Failed to reserve inventory: {ex.Message}");
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<InventoryOperationResult> ConfirmReservationAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return InventoryOperationResult.Failure("Order ID is required");
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Check if reservation exists
                var checkSql = @"
                    SELECT COUNT(*) 
                    FROM Reservations 
                    WHERE OrderId = @OrderId AND IsActive = 1";
                    
                var count = await connection.ExecuteScalarAsync<int>(checkSql, new { OrderId = orderId });
                
                if (count == 0)
                {
                    return InventoryOperationResult.Failure($"No active reservations found for order {orderId}");
                }
                
                // Update reservations
                var updateSql = @"
                    UPDATE Reservations
                    SET IsActive = 0,
                        IsConfirmed = 1,
                        ConfirmationDate = @ConfirmationDate
                    WHERE OrderId = @OrderId AND IsActive = 1";
                    
                await connection.ExecuteAsync(
                    updateSql, 
                    new { OrderId = orderId, ConfirmationDate = DateTime.UtcNow });
                    
                return InventoryOperationResult.Success("Reservation confirmed successfully");
            }
        }

        /// <inheritdoc />
        public async Task<InventoryOperationResult> ReleaseReservationAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return InventoryOperationResult.Failure("Order ID is required");
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Get active reservations
                        var reservationsSql = @"
                            SELECT ItemId, Quantity 
                            FROM Reservations 
                            WHERE OrderId = @OrderId AND IsActive = 1";
                            
                        var reservations = await connection.QueryAsync<ReservationItem>(
                            reservationsSql, 
                            new { OrderId = orderId },
                            transaction);
                            
                        if (!reservations.Any())
                        {
                            return InventoryOperationResult.Failure($"No active reservations found for order {orderId}");
                        }
                        
                        // Update inventory
                        foreach (var reservation in reservations)
                        {
                            var updateSql = @"
                                UPDATE Inventory 
                                SET AvailableQuantity = AvailableQuantity + @Quantity 
                                WHERE Id = @ItemId";
                                
                            await connection.ExecuteAsync(
                                updateSql, 
                                new { ItemId = reservation.ItemId, Quantity = reservation.Quantity },
                                transaction);
                        }
                        
                        // Update reservations
                        var updateReservationSql = @"
                            UPDATE Reservations
                            SET IsActive = 0,
                                ReleaseDate = @ReleaseDate
                            WHERE OrderId = @OrderId AND IsActive = 1";
                            
                        await connection.ExecuteAsync(
                            updateReservationSql, 
                            new { OrderId = orderId, ReleaseDate = DateTime.UtcNow },
                            transaction);
                            
                        await transaction.CommitAsync();
                        
                        return InventoryOperationResult.Success("Reservation released successfully");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return InventoryOperationResult.Failure($"Failed to release reservation: {ex.Message}");
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Get expired reservations
                        var expiredSql = @"
                            SELECT ItemId, Quantity 
                            FROM Reservations 
                            WHERE IsActive = 1 AND ExpiryDate < @CurrentTime";
                            
                        var expiredReservations = await connection.QueryAsync<ReservationItem>(
                            expiredSql, 
                            new { CurrentTime = DateTime.UtcNow },
                            transaction);
                            
                        if (!expiredReservations.Any())
                        {
                            await transaction.CommitAsync();
                            return 0;
                        }
                        
                        // Update inventory
                        foreach (var reservation in expiredReservations)
                        {
                            var updateSql = @"
                                UPDATE Inventory 
                                SET AvailableQuantity = AvailableQuantity + @Quantity 
                                WHERE Id = @ItemId";
                                
                            await connection.ExecuteAsync(
                                updateSql, 
                                new { ItemId = reservation.ItemId, Quantity = reservation.Quantity },
                                transaction);
                        }
                        
                        // Update reservations
                        var updateReservationSql = @"
                            UPDATE Reservations
                            SET IsActive = 0,
                                ReleaseDate = @ReleaseDate
                            WHERE IsActive = 1 AND ExpiryDate < @CurrentTime";
                            
                        var affectedRows = await connection.ExecuteAsync(
                            updateReservationSql, 
                            new { ReleaseDate = DateTime.UtcNow, CurrentTime = DateTime.UtcNow },
                            transaction);
                            
                        await transaction.CommitAsync();
                        
                        return affectedRows;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        return 0;
                    }
                }
            }
        }

        private string BuildWhereClause(string category, string searchTerm)
        {
            var conditions = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(category))
            {
                conditions.Add("i.Category = @Category");
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                conditions.Add("(i.Name LIKE @SearchTerm OR i.Description LIKE @SearchTerm)");
            }
            
            return conditions.Any() 
                ? "WHERE " + string.Join(" AND ", conditions) 
                : string.Empty;
        }
        
        /// <summary>
        /// Helper class for reservation items
        /// </summary>
        private class ReservationItem
        {
            /// <summary>
            /// Gets or sets the item ID
            /// </summary>
            public Guid ItemId { get; set; }
            
            /// <summary>
            /// Gets or sets the quantity
            /// </summary>
            public int Quantity { get; set; }
        }
    }
} 