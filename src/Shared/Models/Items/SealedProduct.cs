using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents a sealed collectible product (booster packs, boxes, decks, etc.)
    /// </summary>
    public class SealedProduct : Item
    {
        /// <summary>
        /// The game or brand the sealed product belongs to
        /// </summary>
        public string Game { get; set; }
        
        /// <summary>
        /// Type of sealed product (e.g., Booster Pack, Booster Box, Starter Deck)
        /// </summary>
        public string ProductType { get; set; }
        
        /// <summary>
        /// The edition or release wave of the product
        /// </summary>
        public string Edition { get; set; }
        
        /// <summary>
        /// Language of the product
        /// </summary>
        public string Language { get; set; }
        
        /// <summary>
        /// Number of items contained in the product (e.g., number of packs in a box)
        /// </summary>
        public int ItemCount { get; set; }
        
        /// <summary>
        /// The year the product was released
        /// </summary>
        public int ReleaseYear { get; set; }
        
        /// <summary>
        /// Whether the product is a first edition release
        /// </summary>
        public bool IsFirstEdition { get; set; }
        
        /// <summary>
        /// Whether the product is a limited release
        /// </summary>
        public bool IsLimited { get; set; }
        
        /// <summary>
        /// Whether the original factory seal is intact
        /// </summary>
        public bool IsFactorySealed { get; set; }
        
        /// <summary>
        /// Any specific packaging details (e.g., display case, sleeve condition)
        /// </summary>
        public string PackagingDetails { get; set; }
        
        /// <summary>
        /// Manufacturer's product code or UPC
        /// </summary>
        public string ManufacturerCode { get; set; }
        
        /// <summary>
        /// Weight of the product in grams
        /// </summary>
        public decimal WeightInGrams { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SealedProduct() : base()
        {
            Category = ItemCategory.SealedProduct;
            IsFactorySealed = true;
        }
    }
} 