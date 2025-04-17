using System;

namespace TCGOrderManagement.InventoryService.Configuration
{
    /// <summary>
    /// Configuration settings for MongoDB connection and collections
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// Gets or sets the MongoDB connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the MongoDB database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection used for inventory items
        /// </summary>
        public string InventoryCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection used for inventory reservations
        /// </summary>
        public string ReservationsCollectionName { get; set; }
    }
} 