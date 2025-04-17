using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents a trading card item with specific card-related properties
    /// </summary>
    public class TradingCard : Item
    {
        /// <summary>
        /// The game or brand the card belongs to (e.g., Magic: The Gathering, Pokémon, Yu-Gi-Oh!)
        /// </summary>
        public string Game { get; set; }
        
        /// <summary>
        /// The expansion set or series the card is from
        /// </summary>
        public string Set { get; set; }
        
        /// <summary>
        /// The card number within its set
        /// </summary>
        public string CardNumber { get; set; }
        
        /// <summary>
        /// The rarity of the card (e.g., Common, Uncommon, Rare, Ultra Rare)
        /// </summary>
        public string Rarity { get; set; }
        
        /// <summary>
        /// Whether the card is foil/holographic
        /// </summary>
        public bool IsFoil { get; set; }
        
        /// <summary>
        /// Whether the card is a special variant (e.g., alternate art, extended art)
        /// </summary>
        public bool IsVariant { get; set; }
        
        /// <summary>
        /// Specific variant type if applicable (e.g., Full Art, Borderless)
        /// </summary>
        public string VariantType { get; set; }
        
        /// <summary>
        /// Card artist name
        /// </summary>
        public string Artist { get; set; }
        
        /// <summary>
        /// Year the card was published
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// The language of the card
        /// </summary>
        public string Language { get; set; }
        
        /// <summary>
        /// Professional grading company name if graded (e.g., PSA, BGS)
        /// </summary>
        public string GradingCompany { get; set; }
        
        /// <summary>
        /// Numeric grade if professionally graded
        /// </summary>
        public decimal? GradeValue { get; set; }
        
        /// <summary>
        /// Certification/serial number from grading company
        /// </summary>
        public string GradingCertNumber { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TradingCard() : base()
        {
            Category = ItemCategory.TradingCard;
        }
    }
} 