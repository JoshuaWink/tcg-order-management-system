using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TCGOrderManagement.Shared.Models.Items;
using TCGOrderManagement.Shared.Repositories;

namespace TCGOrderManagement.InventoryService.Repositories
{
    /// <summary>
    /// SQL Server implementation of the IItemRepository interface
    /// </summary>
    public class SqlItemRepository : IItemRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public SqlItemRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Creates and returns a new SQL connection
        /// </summary>
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <inheritdoc />
        public async Task<Item> GetByIdAsync(Guid id)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                // First get the base item data
                var sql = @"
                    SELECT Id, Sku, Name, Description, Category, Condition, Price, 
                           QuantityAvailable, ImageUrl, SetName, SetIdentifier, 
                           CreatedAt, UpdatedAt, IsActive
                    FROM Items 
                    WHERE Id = @Id";

                var item = await connection.QueryFirstOrDefaultAsync<Item>(sql, new { Id = id });

                if (item == null)
                    return null;

                // Get attributes for the item
                var attributesSql = "SELECT [Key], [Value] FROM ItemAttributes WHERE ItemId = @ItemId";
                var attributes = await connection.QueryAsync<KeyValuePair<string, string>>(attributesSql, new { ItemId = id });
                
                item.Attributes = attributes.ToDictionary(a => a.Key, a => a.Value);

                // Based on the category, load the specific item type
                switch (item.Category)
                {
                    case ItemCategory.TradingCard:
                        return await LoadTradingCardAsync(connection, item);
                    case ItemCategory.SealedProduct:
                        return await LoadSealedProductAsync(connection, item);
                    default:
                        return item;
                }
            }
        }

        /// <inheritdoc />
        public async Task<Item> GetBySkuAsync(string sku)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                var sql = "SELECT Id FROM Items WHERE Sku = @Sku";
                var id = await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { Sku = sku });

                if (!id.HasValue)
                    return null;

                return await GetByIdAsync(id.Value);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetAllAsync(
            ItemCategory? category = null,
            ItemCondition? condition = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT Id, Sku, Name, Description, Category, Condition, Price, 
                           QuantityAvailable, ImageUrl, SetName, SetIdentifier, 
                           CreatedAt, UpdatedAt, IsActive
                    FROM Items
                    WHERE 1=1";

                var parameters = new DynamicParameters();

                if (category.HasValue)
                {
                    sql += " AND Category = @Category";
                    parameters.Add("Category", (int)category.Value);
                }

                if (condition.HasValue)
                {
                    sql += " AND Condition = @Condition";
                    parameters.Add("Condition", (int)condition.Value);
                }

                if (minPrice.HasValue)
                {
                    sql += " AND Price >= @MinPrice";
                    parameters.Add("MinPrice", minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    sql += " AND Price <= @MaxPrice";
                    parameters.Add("MaxPrice", maxPrice.Value);
                }

                if (isActive.HasValue)
                {
                    sql += " AND IsActive = @IsActive";
                    parameters.Add("IsActive", isActive.Value);
                }

                var items = await connection.QueryAsync<Item>(sql, parameters);

                // Load additional data for each item
                var result = new List<Item>();
                foreach (var item in items)
                {
                    // Load attributes
                    var attributesSql = "SELECT [Key], [Value] FROM ItemAttributes WHERE ItemId = @ItemId";
                    var attributes = await connection.QueryAsync<KeyValuePair<string, string>>(attributesSql, new { ItemId = item.Id });
                    item.Attributes = attributes.ToDictionary(a => a.Key, a => a.Value);

                    // Based on category, load specific data
                    switch (item.Category)
                    {
                        case ItemCategory.TradingCard:
                            result.Add(await LoadTradingCardAsync(connection, item));
                            break;
                        case ItemCategory.SealedProduct:
                            result.Add(await LoadSealedProductAsync(connection, item));
                            break;
                        default:
                            result.Add(item);
                            break;
                    }
                }

                return result;
            }
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Item> Items, int TotalCount)> SearchAsync(
            string searchTerm,
            ItemCategory? category = null,
            ItemCondition? condition = null,
            int page = 1,
            int pageSize = 20)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                // Build the WHERE clause for filtering
                var whereClause = "WHERE (Name LIKE @SearchTerm OR Description LIKE @SearchTerm OR Sku LIKE @SearchTerm)";
                var parameters = new DynamicParameters();
                parameters.Add("SearchTerm", $"%{searchTerm}%");
                parameters.Add("Offset", (page - 1) * pageSize);
                parameters.Add("PageSize", pageSize);

                if (category.HasValue)
                {
                    whereClause += " AND Category = @Category";
                    parameters.Add("Category", (int)category.Value);
                }

                if (condition.HasValue)
                {
                    whereClause += " AND Condition = @Condition";
                    parameters.Add("Condition", (int)condition.Value);
                }

                // Count query
                var countSql = $"SELECT COUNT(*) FROM Items {whereClause}";
                var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Items query with pagination
                var sql = $@"
                    SELECT Id, Sku, Name, Description, Category, Condition, Price, 
                           QuantityAvailable, ImageUrl, SetName, SetIdentifier, 
                           CreatedAt, UpdatedAt, IsActive
                    FROM Items
                    {whereClause}
                    ORDER BY Name
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var items = await connection.QueryAsync<Item>(sql, parameters);

                // Load additional data for each item
                var result = new List<Item>();
                foreach (var item in items)
                {
                    // Load attributes
                    var attributesSql = "SELECT [Key], [Value] FROM ItemAttributes WHERE ItemId = @ItemId";
                    var attributes = await connection.QueryAsync<KeyValuePair<string, string>>(attributesSql, new { ItemId = item.Id });
                    item.Attributes = attributes.ToDictionary(a => a.Key, a => a.Value);

                    // Based on category, load specific data
                    switch (item.Category)
                    {
                        case ItemCategory.TradingCard:
                            result.Add(await LoadTradingCardAsync(connection, item));
                            break;
                        case ItemCategory.SealedProduct:
                            result.Add(await LoadSealedProductAsync(connection, item));
                            break;
                        default:
                            result.Add(item);
                            break;
                    }
                }

                return (result, totalCount);
            }
        }

        /// <inheritdoc />
        public async Task<Item> AddAsync(Item item)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Ensure the item has an ID
                        if (item.Id == Guid.Empty)
                        {
                            item.Id = Guid.NewGuid();
                        }

                        // Set timestamps
                        item.CreatedAt = DateTime.UtcNow;
                        item.UpdatedAt = DateTime.UtcNow;

                        // Insert base item data
                        var sql = @"
                            INSERT INTO Items (Id, Sku, Name, Description, Category, Condition, 
                                             Price, QuantityAvailable, ImageUrl, SetName, 
                                             SetIdentifier, CreatedAt, UpdatedAt, IsActive)
                            VALUES (@Id, @Sku, @Name, @Description, @Category, @Condition,
                                   @Price, @QuantityAvailable, @ImageUrl, @SetName,
                                   @SetIdentifier, @CreatedAt, @UpdatedAt, @IsActive)";

                        await connection.ExecuteAsync(sql, item, transaction);

                        // Insert attributes
                        if (item.Attributes != null && item.Attributes.Count > 0)
                        {
                            var attributesSql = @"
                                INSERT INTO ItemAttributes (ItemId, [Key], [Value])
                                VALUES (@ItemId, @Key, @Value)";

                            foreach (var attribute in item.Attributes)
                            {
                                await connection.ExecuteAsync(attributesSql, 
                                    new { ItemId = item.Id, Key = attribute.Key, Value = attribute.Value }, 
                                    transaction);
                            }
                        }

                        // Insert type-specific data
                        switch (item.Category)
                        {
                            case ItemCategory.TradingCard:
                                await InsertTradingCardAsync(connection, transaction, item as TradingCard);
                                break;
                            case ItemCategory.SealedProduct:
                                await InsertSealedProductAsync(connection, transaction, item as SealedProduct);
                                break;
                        }

                        transaction.Commit();
                        return item;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<Item> UpdateAsync(Item item)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update timestamp
                        item.UpdatedAt = DateTime.UtcNow;

                        // Update base item data
                        var sql = @"
                            UPDATE Items
                            SET Sku = @Sku,
                                Name = @Name,
                                Description = @Description,
                                Category = @Category,
                                Condition = @Condition,
                                Price = @Price,
                                QuantityAvailable = @QuantityAvailable,
                                ImageUrl = @ImageUrl,
                                SetName = @SetName,
                                SetIdentifier = @SetIdentifier,
                                UpdatedAt = @UpdatedAt,
                                IsActive = @IsActive
                            WHERE Id = @Id";

                        await connection.ExecuteAsync(sql, item, transaction);

                        // Update attributes - first delete existing
                        var deleteAttributesSql = "DELETE FROM ItemAttributes WHERE ItemId = @ItemId";
                        await connection.ExecuteAsync(deleteAttributesSql, new { ItemId = item.Id }, transaction);

                        // Then insert new attributes
                        if (item.Attributes != null && item.Attributes.Count > 0)
                        {
                            var attributesSql = @"
                                INSERT INTO ItemAttributes (ItemId, [Key], [Value])
                                VALUES (@ItemId, @Key, @Value)";

                            foreach (var attribute in item.Attributes)
                            {
                                await connection.ExecuteAsync(attributesSql,
                                    new { ItemId = item.Id, Key = attribute.Key, Value = attribute.Value },
                                    transaction);
                            }
                        }

                        // Update type-specific data
                        switch (item.Category)
                        {
                            case ItemCategory.TradingCard:
                                await UpdateTradingCardAsync(connection, transaction, item as TradingCard);
                                break;
                            case ItemCategory.SealedProduct:
                                await UpdateSealedProductAsync(connection, transaction, item as SealedProduct);
                                break;
                        }

                        transaction.Commit();
                        return item;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First get the item to determine its type
                        var sql = "SELECT Category FROM Items WHERE Id = @Id";
                        var category = await connection.QueryFirstOrDefaultAsync<ItemCategory?>(sql, new { Id = id }, transaction);

                        if (!category.HasValue)
                            return false;

                        // Delete type-specific data
                        switch (category.Value)
                        {
                            case ItemCategory.TradingCard:
                                await connection.ExecuteAsync("DELETE FROM TradingCards WHERE ItemId = @ItemId", 
                                    new { ItemId = id }, transaction);
                                break;
                            case ItemCategory.SealedProduct:
                                await connection.ExecuteAsync("DELETE FROM SealedProducts WHERE ItemId = @ItemId", 
                                    new { ItemId = id }, transaction);
                                break;
                        }

                        // Delete attributes
                        await connection.ExecuteAsync("DELETE FROM ItemAttributes WHERE ItemId = @ItemId", 
                            new { ItemId = id }, transaction);

                        // Delete base item
                        await connection.ExecuteAsync("DELETE FROM Items WHERE Id = @Id", 
                            new { Id = id }, transaction);

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

        /// <inheritdoc />
        public async Task<int> UpdateInventoryQuantityAsync(Guid id, int quantityChange)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                // Update the quantity and return the new value
                var sql = @"
                    UPDATE Items
                    SET QuantityAvailable = QuantityAvailable + @QuantityChange,
                        UpdatedAt = @UpdatedAt
                    OUTPUT INSERTED.QuantityAvailable
                    WHERE Id = @Id";

                return await connection.ExecuteScalarAsync<int>(sql, 
                    new { Id = id, QuantityChange = quantityChange, UpdatedAt = DateTime.UtcNow });
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid id)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                var sql = "SELECT COUNT(1) FROM Items WHERE Id = @Id";
                return await connection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
            }
        }

        #region Helper Methods

        private async Task<TradingCard> LoadTradingCardAsync(IDbConnection connection, Item baseItem)
        {
            var sql = @"
                SELECT Game, Set, CardNumber, Rarity, IsFoil, IsVariant, VariantType,
                       Artist, Year, Language, GradingCompany, GradeValue, GradingCertNumber
                FROM TradingCards
                WHERE ItemId = @ItemId";

            var cardData = await connection.QueryFirstOrDefaultAsync<TradingCard>(sql, new { ItemId = baseItem.Id });

            if (cardData == null)
                return null;

            // Copy base properties
            cardData.Id = baseItem.Id;
            cardData.Sku = baseItem.Sku;
            cardData.Name = baseItem.Name;
            cardData.Description = baseItem.Description;
            cardData.Category = baseItem.Category;
            cardData.Condition = baseItem.Condition;
            cardData.Price = baseItem.Price;
            cardData.QuantityAvailable = baseItem.QuantityAvailable;
            cardData.ImageUrl = baseItem.ImageUrl;
            cardData.SetName = baseItem.SetName;
            cardData.SetIdentifier = baseItem.SetIdentifier;
            cardData.CreatedAt = baseItem.CreatedAt;
            cardData.UpdatedAt = baseItem.UpdatedAt;
            cardData.IsActive = baseItem.IsActive;
            cardData.Attributes = baseItem.Attributes;

            return cardData;
        }

        private async Task<SealedProduct> LoadSealedProductAsync(IDbConnection connection, Item baseItem)
        {
            var sql = @"
                SELECT Game, ProductType, Edition, Language, ItemCount, ReleaseYear,
                       IsFirstEdition, IsLimited, IsFactorySealed, PackagingDetails,
                       ManufacturerCode, WeightInGrams
                FROM SealedProducts
                WHERE ItemId = @ItemId";

            var productData = await connection.QueryFirstOrDefaultAsync<SealedProduct>(sql, new { ItemId = baseItem.Id });

            if (productData == null)
                return null;

            // Copy base properties
            productData.Id = baseItem.Id;
            productData.Sku = baseItem.Sku;
            productData.Name = baseItem.Name;
            productData.Description = baseItem.Description;
            productData.Category = baseItem.Category;
            productData.Condition = baseItem.Condition;
            productData.Price = baseItem.Price;
            productData.QuantityAvailable = baseItem.QuantityAvailable;
            productData.ImageUrl = baseItem.ImageUrl;
            productData.SetName = baseItem.SetName;
            productData.SetIdentifier = baseItem.SetIdentifier;
            productData.CreatedAt = baseItem.CreatedAt;
            productData.UpdatedAt = baseItem.UpdatedAt;
            productData.IsActive = baseItem.IsActive;
            productData.Attributes = baseItem.Attributes;

            return productData;
        }

        private async Task InsertTradingCardAsync(IDbConnection connection, IDbTransaction transaction, TradingCard card)
        {
            if (card == null)
                return;

            var sql = @"
                INSERT INTO TradingCards (ItemId, Game, Set, CardNumber, Rarity, IsFoil, 
                                       IsVariant, VariantType, Artist, Year, Language,
                                       GradingCompany, GradeValue, GradingCertNumber)
                VALUES (@ItemId, @Game, @Set, @CardNumber, @Rarity, @IsFoil,
                       @IsVariant, @VariantType, @Artist, @Year, @Language,
                       @GradingCompany, @GradeValue, @GradingCertNumber)";

            await connection.ExecuteAsync(sql, new
            {
                ItemId = card.Id,
                card.Game,
                Set = card.Set,
                card.CardNumber,
                card.Rarity,
                card.IsFoil,
                card.IsVariant,
                card.VariantType,
                card.Artist,
                card.Year,
                card.Language,
                card.GradingCompany,
                card.GradeValue,
                card.GradingCertNumber
            }, transaction);
        }

        private async Task InsertSealedProductAsync(IDbConnection connection, IDbTransaction transaction, SealedProduct product)
        {
            if (product == null)
                return;

            var sql = @"
                INSERT INTO SealedProducts (ItemId, Game, ProductType, Edition, Language, 
                                         ItemCount, ReleaseYear, IsFirstEdition, IsLimited,
                                         IsFactorySealed, PackagingDetails, ManufacturerCode,
                                         WeightInGrams)
                VALUES (@ItemId, @Game, @ProductType, @Edition, @Language,
                       @ItemCount, @ReleaseYear, @IsFirstEdition, @IsLimited,
                       @IsFactorySealed, @PackagingDetails, @ManufacturerCode,
                       @WeightInGrams)";

            await connection.ExecuteAsync(sql, new
            {
                ItemId = product.Id,
                product.Game,
                product.ProductType,
                product.Edition,
                product.Language,
                product.ItemCount,
                product.ReleaseYear,
                product.IsFirstEdition,
                product.IsLimited,
                product.IsFactorySealed,
                product.PackagingDetails,
                product.ManufacturerCode,
                product.WeightInGrams
            }, transaction);
        }

        private async Task UpdateTradingCardAsync(IDbConnection connection, IDbTransaction transaction, TradingCard card)
        {
            if (card == null)
                return;

            // First delete existing record
            await connection.ExecuteAsync("DELETE FROM TradingCards WHERE ItemId = @ItemId", 
                new { ItemId = card.Id }, transaction);

            // Then insert new record
            await InsertTradingCardAsync(connection, transaction, card);
        }

        private async Task UpdateSealedProductAsync(IDbConnection connection, IDbTransaction transaction, SealedProduct product)
        {
            if (product == null)
                return;

            // First delete existing record
            await connection.ExecuteAsync("DELETE FROM SealedProducts WHERE ItemId = @ItemId", 
                new { ItemId = product.Id }, transaction);

            // Then insert new record
            await InsertSealedProductAsync(connection, transaction, product);
        }

        #endregion
    }
} 