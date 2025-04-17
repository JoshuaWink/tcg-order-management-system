using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents the condition of a collectible item
    /// </summary>
    public enum ItemCondition
    {
        /// <summary>
        /// Unknown or unspecified condition
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Poor condition - heavily played, damaged, or defective
        /// </summary>
        Poor = 10,
        
        /// <summary>
        /// Damaged condition but still collectible
        /// </summary>
        Damaged = 20,
        
        /// <summary>
        /// Heavily played condition with significant wear
        /// </summary>
        HeavilyPlayed = 30,
        
        /// <summary>
        /// Moderately played condition with moderate wear
        /// </summary>
        ModeratelyPlayed = 40,
        
        /// <summary>
        /// Lightly played condition with minor wear
        /// </summary>
        LightlyPlayed = 50,
        
        /// <summary>
        /// Excellent condition with minimal wear
        /// </summary>
        Excellent = 60,
        
        /// <summary>
        /// Near Mint condition - almost perfect with very minor imperfections
        /// </summary>
        NearMint = 70,
        
        /// <summary>
        /// Mint condition - perfect or nearly perfect condition
        /// </summary>
        Mint = 80,
        
        /// <summary>
        /// Gem Mint condition - absolutely perfect condition
        /// </summary>
        GemMint = 90,
        
        /// <summary>
        /// Factory sealed condition - unopened, in original packaging
        /// </summary>
        Sealed = 100
    }
} 