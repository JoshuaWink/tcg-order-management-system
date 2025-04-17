using System;
using TCGOrderManagement.Shared.Models.Items;

namespace TCGOrderManagement.InventoryService.Models
{
    /// <summary>
    /// Represents an inventory item in the system
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the inventory item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the seller/owner identifier of this inventory item
        /// </summary>
        public Guid SellerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the card
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Gets or sets the set name the card belongs to
        /// </summary>
        public string SetName { get; set; }

        /// <summary>
        /// Gets or sets the set code
        /// </summary>
        public string SetCode { get; set; }

        /// <summary>
        /// Gets or sets the collector number
        /// </summary>
        public string CollectorNumber { get; set; }

        /// <summary>
        /// Gets or sets the rarity of the card
        /// </summary>
        public string Rarity { get; set; }

        /// <summary>
        /// Gets or sets the condition of the card
        /// </summary>
        public ItemCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the card is foil
        /// </summary>
        public bool IsFoil { get; set; }

        /// <summary>
        /// Gets or sets the language of the card
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets additional seller notes about the item
        /// </summary>
        public string SellerNotes { get; set; }

        /// <summary>
        /// Gets or sets the price of the item in cents
        /// </summary>
        public int PriceCents { get; set; }

        /// <summary>
        /// Gets or sets the available quantity of this item
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Gets or sets the reserved quantity of this item
        /// </summary>
        public int ReservedQuantity { get; set; }

        /// <summary>
        /// Gets or sets the path to the primary image of the item
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this item was listed
        /// </summary>
        public DateTime ListedDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this item was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
} 