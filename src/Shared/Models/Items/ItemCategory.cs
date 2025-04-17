using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents categories of collectible items
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>
        /// Unspecified or other category
        /// </summary>
        Other = 0,
        
        /// <summary>
        /// Trading cards (TCG cards, sports cards, etc.)
        /// </summary>
        TradingCard = 1,
        
        /// <summary>
        /// Sealed products (booster packs, boxes, etc.)
        /// </summary>
        SealedProduct = 2,
        
        /// <summary>
        /// Gaming accessories (sleeves, playmats, etc.)
        /// </summary>
        Accessory = 3,
        
        /// <summary>
        /// Collectible figurines and statues
        /// </summary>
        Figurine = 4,
        
        /// <summary>
        /// Memorabilia items
        /// </summary>
        Memorabilia = 5,
        
        /// <summary>
        /// Comic books and graphic novels
        /// </summary>
        ComicBook = 6,
        
        /// <summary>
        /// Vintage collectibles
        /// </summary>
        Vintage = 7,
        
        /// <summary>
        /// Limited edition or exclusive items
        /// </summary>
        LimitedEdition = 8,
        
        /// <summary>
        /// Signed or autographed items
        /// </summary>
        Autographed = 9
    }
} 