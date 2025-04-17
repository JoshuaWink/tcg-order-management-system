using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using TCGOrderManagement.Shared.Models.Orders;
using TCGOrderManagement.Shared.Repositories;

namespace TCGOrderManagement.OrderService.Repositories
{
    /// <summary>
    /// SQL Server implementation of the order repository
    /// </summary>
    public class SqlOrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlOrderRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the SqlOrderRepository class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="logger">Logger instance</param>
        public SqlOrderRepository(string connectionString, ILogger<SqlOrderRepository> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Order> GetByIdAsync(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var orderQuery = @"
                        SELECT o.*, 
                               oi.Id as ItemId, oi.OrderId, oi.ItemId as InventoryItemId, oi.Quantity, 
                               oi.UnitPrice, oi.Condition, oi.Discount
                        FROM Orders o
                        LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                        WHERE o.Id = @Id";

                    var orderDictionary = new Dictionary<Guid, Order>();

                    var result = await connection.QueryAsync<Order, OrderItem, Order>(
                        orderQuery,
                        (order, orderItem) =>
                        {
                            if (!orderDictionary.TryGetValue(order.Id, out var orderEntry))
                            {
                                orderEntry = order;
                                orderEntry.Items = new List<OrderItem>();
                                orderDictionary.Add(order.Id, orderEntry);
                            }

                            if (orderItem != null)
                            {
                                orderEntry.Items.Add(orderItem);
                            }

                            return orderEntry;
                        },
                        new { Id = id },
                        splitOn: "ItemId");

                    var order = orderDictionary.Values.FirstOrDefault();

                    if (order != null)
                    {
                        // Load status history
                        order.StatusHistory = await GetStatusHistoryAsync(id);
                    }

                    return order;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving order with ID {OrderId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByCustomerIdAsync(
            Guid customerId,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get total count
                    var countQuery = "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId";
                    var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, new { CustomerId = customerId });

                    // Get paged results
                    var offset = (page - 1) * pageSize;
                    var ordersQuery = @"
                        SELECT o.* FROM Orders o
                        WHERE o.CustomerId = @CustomerId
                        ORDER BY o.OrderDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    var orders = await connection.QueryAsync<Order>(
                        ordersQuery,
                        new 
                        { 
                            CustomerId = customerId,
                            Offset = offset,
                            PageSize = pageSize
                        });

                    // For each order, get the items
                    var ordersList = orders.ToList();
                    if (ordersList.Any())
                    {
                        var orderIds = ordersList.Select(o => o.Id).ToList();
                        var itemsQuery = "SELECT * FROM OrderItems WHERE OrderId IN @OrderIds";
                        var items = await connection.QueryAsync<OrderItem>(itemsQuery, new { OrderIds = orderIds });

                        var itemsByOrderId = items.GroupBy(i => i.OrderId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        foreach (var order in ordersList)
                        {
                            if (itemsByOrderId.TryGetValue(order.Id, out var orderItems))
                            {
                                order.Items = orderItems;
                            }
                            else
                            {
                                order.Items = new List<OrderItem>();
                            }
                        }
                    }

                    return (ordersList, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByStatusAsync(
            OrderStatus status,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get total count
                    var countQuery = "SELECT COUNT(*) FROM Orders WHERE Status = @Status";
                    var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, new { Status = (int)status });

                    // Get paged results
                    var offset = (page - 1) * pageSize;
                    var ordersQuery = @"
                        SELECT o.* FROM Orders o
                        WHERE o.Status = @Status
                        ORDER BY o.OrderDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    var orders = await connection.QueryAsync<Order>(
                        ordersQuery,
                        new 
                        { 
                            Status = (int)status,
                            Offset = offset,
                            PageSize = pageSize
                        });

                    // For each order, get the items
                    var ordersList = orders.ToList();
                    
                    if (ordersList.Any())
                    {
                        var orderIds = ordersList.Select(o => o.Id).ToList();
                        var itemsQuery = "SELECT * FROM OrderItems WHERE OrderId IN @OrderIds";
                        var items = await connection.QueryAsync<OrderItem>(itemsQuery, new { OrderIds = orderIds });

                        var itemsByOrderId = items.GroupBy(i => i.OrderId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        foreach (var order in ordersList)
                        {
                            if (itemsByOrderId.TryGetValue(order.Id, out var orderItems))
                            {
                                order.Items = orderItems;
                            }
                            else
                            {
                                order.Items = new List<OrderItem>();
                            }
                        }
                    }

                    return (ordersList, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving orders with status {Status}", status);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get total count
                    var countQuery = @"
                        SELECT COUNT(*) FROM Orders 
                        WHERE OrderDate BETWEEN @StartDate AND @EndDate";
                    
                    var totalCount = await connection.ExecuteScalarAsync<int>(
                        countQuery, 
                        new { StartDate = startDate, EndDate = endDate });

                    // Get paged results
                    var offset = (page - 1) * pageSize;
                    var ordersQuery = @"
                        SELECT o.* FROM Orders o
                        WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                        ORDER BY o.OrderDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    var orders = await connection.QueryAsync<Order>(
                        ordersQuery,
                        new 
                        { 
                            StartDate = startDate,
                            EndDate = endDate,
                            Offset = offset,
                            PageSize = pageSize
                        });

                    var ordersList = orders.ToList();
                    if (ordersList.Any())
                    {
                        await LoadOrderItems(connection, ordersList);
                    }

                    return (ordersList, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving orders between {StartDate} and {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> SearchAsync(
            string searchTerm,
            OrderStatus? status = null,
            Guid? customerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var whereClauses = new List<string>();
                    var parameters = new DynamicParameters();

                    // Add search term filter
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        whereClauses.Add("(o.OrderNumber LIKE @SearchTerm OR o.CustomerNotes LIKE @SearchTerm)");
                        parameters.Add("SearchTerm", $"%{searchTerm}%");
                    }

                    // Add status filter
                    if (status.HasValue)
                    {
                        whereClauses.Add("o.Status = @Status");
                        parameters.Add("Status", (int)status.Value);
                    }

                    // Add customer filter
                    if (customerId.HasValue)
                    {
                        whereClauses.Add("o.CustomerId = @CustomerId");
                        parameters.Add("CustomerId", customerId.Value);
                    }

                    // Add date range filter
                    if (startDate.HasValue)
                    {
                        whereClauses.Add("o.OrderDate >= @StartDate");
                        parameters.Add("StartDate", startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        whereClauses.Add("o.OrderDate <= @EndDate");
                        parameters.Add("EndDate", endDate.Value);
                    }

                    // Build where clause
                    var whereClause = whereClauses.Any() 
                        ? $"WHERE {string.Join(" AND ", whereClauses)}" 
                        : string.Empty;

                    // Get total count
                    var countQuery = $"SELECT COUNT(*) FROM Orders o {whereClause}";
                    var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, parameters);

                    // Get paged results
                    var offset = (page - 1) * pageSize;
                    var ordersQuery = $@"
                        SELECT o.* FROM Orders o
                        {whereClause}
                        ORDER BY o.OrderDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    parameters.Add("Offset", offset);
                    parameters.Add("PageSize", pageSize);

                    var orders = await connection.QueryAsync<Order>(ordersQuery, parameters);

                    var ordersList = orders.ToList();
                    if (ordersList.Any())
                    {
                        await LoadOrderItems(connection, ordersList);
                    }

                    return (ordersList, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching orders with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Order> AddAsync(Order order)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert order
                            var insertOrderQuery = @"
                                INSERT INTO Orders (
                                    Id, CustomerId, OrderNumber, OrderDate, Status, 
                                    ShippingAddress, BillingAddress, PaymentMethod, 
                                    SubTotal, ShippingCost, Tax, TotalCost, 
                                    CustomerNotes, InternalNotes, TrackingNumber, 
                                    PaymentStatus, ShippingStatus, LastUpdated, CreatedBy)
                                VALUES (
                                    @Id, @CustomerId, @OrderNumber, @OrderDate, @Status, 
                                    @ShippingAddress, @BillingAddress, @PaymentMethod, 
                                    @SubTotal, @ShippingCost, @Tax, @TotalCost, 
                                    @CustomerNotes, @InternalNotes, @TrackingNumber, 
                                    @PaymentStatus, @ShippingStatus, @LastUpdated, @CreatedBy);";

                            await connection.ExecuteAsync(insertOrderQuery, order, transaction);

                            // Insert order items
                            if (order.Items != null && order.Items.Any())
                            {
                                var insertItemsQuery = @"
                                    INSERT INTO OrderItems (
                                        Id, OrderId, ItemId, Quantity, UnitPrice, 
                                        Condition, Discount)
                                    VALUES (
                                        @Id, @OrderId, @ItemId, @Quantity, @UnitPrice, 
                                        @Condition, @Discount);";

                                foreach (var item in order.Items)
                                {
                                    if (item.Id == Guid.Empty)
                                    {
                                        item.Id = Guid.NewGuid();
                                    }
                                    
                                    item.OrderId = order.Id;
                                    await connection.ExecuteAsync(insertItemsQuery, item, transaction);
                                }
                            }

                            // Insert initial status history
                            var insertStatusQuery = @"
                                INSERT INTO OrderStatusHistory (
                                    Id, OrderId, Status, Comment, ChangedAt, ChangedBy)
                                VALUES (
                                    @Id, @OrderId, @Status, @Comment, @ChangedAt, @ChangedBy);";

                            var statusHistory = new OrderStatusHistory
                            {
                                Id = Guid.NewGuid(),
                                OrderId = order.Id,
                                Status = order.Status,
                                Comment = "Order created",
                                ChangedAt = DateTime.UtcNow,
                                ChangedBy = order.CreatedBy
                            };

                            await connection.ExecuteAsync(insertStatusQuery, statusHistory, transaction);

                            transaction.Commit();
                            return order;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding order {OrderId}", order.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Order> UpdateAsync(Order order)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Update order
                            var updateOrderQuery = @"
                                UPDATE Orders SET
                                    CustomerId = @CustomerId,
                                    OrderNumber = @OrderNumber,
                                    OrderDate = @OrderDate,
                                    Status = @Status,
                                    ShippingAddress = @ShippingAddress,
                                    BillingAddress = @BillingAddress,
                                    PaymentMethod = @PaymentMethod,
                                    SubTotal = @SubTotal,
                                    ShippingCost = @ShippingCost,
                                    Tax = @Tax,
                                    TotalCost = @TotalCost,
                                    CustomerNotes = @CustomerNotes,
                                    InternalNotes = @InternalNotes,
                                    TrackingNumber = @TrackingNumber,
                                    PaymentStatus = @PaymentStatus,
                                    ShippingStatus = @ShippingStatus,
                                    LastUpdated = @LastUpdated
                                WHERE Id = @Id";

                            // Update last updated timestamp
                            order.LastUpdated = DateTime.UtcNow;

                            await connection.ExecuteAsync(updateOrderQuery, order, transaction);

                            // Delete existing items
                            var deleteItemsQuery = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
                            await connection.ExecuteAsync(deleteItemsQuery, new { OrderId = order.Id }, transaction);

                            // Insert updated items
                            if (order.Items != null && order.Items.Any())
                            {
                                var insertItemsQuery = @"
                                    INSERT INTO OrderItems (
                                        Id, OrderId, ItemId, Quantity, UnitPrice, 
                                        Condition, Discount)
                                    VALUES (
                                        @Id, @OrderId, @ItemId, @Quantity, @UnitPrice, 
                                        @Condition, @Discount);";

                                foreach (var item in order.Items)
                                {
                                    if (item.Id == Guid.Empty)
                                    {
                                        item.Id = Guid.NewGuid();
                                    }
                                    
                                    item.OrderId = order.Id;
                                    await connection.ExecuteAsync(insertItemsQuery, item, transaction);
                                }
                            }

                            transaction.Commit();
                            return order;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating order {OrderId}", order.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, string comment = null, string changedBy = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Update order status
                            var updateStatusQuery = @"
                                UPDATE Orders SET
                                    Status = @Status,
                                    LastUpdated = @LastUpdated
                                WHERE Id = @Id";

                            await connection.ExecuteAsync(
                                updateStatusQuery, 
                                new 
                                { 
                                    Status = (int)status,
                                    LastUpdated = DateTime.UtcNow,
                                    Id = id 
                                }, 
                                transaction);

                            // Add status history record
                            var insertStatusQuery = @"
                                INSERT INTO OrderStatusHistory (
                                    Id, OrderId, Status, Comment, ChangedAt, ChangedBy)
                                VALUES (
                                    @Id, @OrderId, @Status, @Comment, @ChangedAt, @ChangedBy);";

                            var statusHistory = new OrderStatusHistory
                            {
                                Id = Guid.NewGuid(),
                                OrderId = id,
                                Status = status,
                                Comment = comment,
                                ChangedAt = DateTime.UtcNow,
                                ChangedBy = changedBy ?? "System"
                            };

                            await connection.ExecuteAsync(insertStatusQuery, statusHistory, transaction);

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating status for order {OrderId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var statusQuery = @"
                        SELECT * FROM OrderStatusHistory
                        WHERE OrderId = @OrderId
                        ORDER BY ChangedAt DESC";

                    return await connection.QueryAsync<OrderStatusHistory>(statusQuery, new { OrderId = orderId });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving status history for order {OrderId}", orderId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersContainingItemAsync(
            Guid itemId,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get total count
                    var countQuery = @"
                        SELECT COUNT(DISTINCT o.Id) 
                        FROM Orders o
                        JOIN OrderItems oi ON o.Id = oi.OrderId
                        WHERE oi.ItemId = @ItemId";
                    
                    var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, new { ItemId = itemId });

                    // Get paged results
                    var offset = (page - 1) * pageSize;
                    var ordersQuery = @"
                        SELECT DISTINCT o.* 
                        FROM Orders o
                        JOIN OrderItems oi ON o.Id = oi.OrderId
                        WHERE oi.ItemId = @ItemId
                        ORDER BY o.OrderDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    var orders = await connection.QueryAsync<Order>(
                        ordersQuery,
                        new 
                        { 
                            ItemId = itemId,
                            Offset = offset,
                            PageSize = pageSize
                        });

                    var ordersList = orders.ToList();
                    if (ordersList.Any())
                    {
                        await LoadOrderItems(connection, ordersList);
                    }

                    return (ordersList, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving orders containing item {ItemId}", itemId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // First delete related records
                            var deleteItemsQuery = "DELETE FROM OrderItems WHERE OrderId = @Id";
                            await connection.ExecuteAsync(deleteItemsQuery, new { Id = id }, transaction);

                            var deleteHistoryQuery = "DELETE FROM OrderStatusHistory WHERE OrderId = @Id";
                            await connection.ExecuteAsync(deleteHistoryQuery, new { Id = id }, transaction);

                            // Then delete the order itself
                            var deleteOrderQuery = "DELETE FROM Orders WHERE Id = @Id";
                            var result = await connection.ExecuteAsync(deleteOrderQuery, new { Id = id }, transaction);

                            transaction.Commit();
                            return result > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting order {OrderId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT COUNT(1) FROM Orders WHERE Id = @Id";
                    var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if order {OrderId} exists", id);
                throw;
            }
        }

        /// <summary>
        /// Get the total count of orders for a customer
        /// </summary>
        public async Task<int> GetCustomerOrderCountAsync(Guid customerId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId";
                    return await connection.ExecuteScalarAsync<int>(query, new { CustomerId = customerId });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting order count for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Loads order items for a list of orders
        /// </summary>
        private async Task LoadOrderItems(IDbConnection connection, List<Order> orders)
        {
            if (!orders.Any())
                return;

            var orderIds = orders.Select(o => o.Id).ToList();
            var itemsQuery = "SELECT * FROM OrderItems WHERE OrderId IN @OrderIds";
            var items = await connection.QueryAsync<OrderItem>(itemsQuery, new { OrderIds = orderIds });

            var itemsByOrderId = items.GroupBy(i => i.OrderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var order in orders)
            {
                if (itemsByOrderId.TryGetValue(order.Id, out var orderItems))
                {
                    order.Items = orderItems;
                }
                else
                {
                    order.Items = new List<OrderItem>();
                }
            }
        }
    }
} 