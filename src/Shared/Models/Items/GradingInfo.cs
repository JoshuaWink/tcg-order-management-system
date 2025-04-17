using System;

namespace TCGOrderManagement.Shared.Models.Items
{
    /// <summary>
    /// Represents information about a professionally graded item
    /// </summary>
    public class GradingInfo
    {
        /// <summary>
        /// The company that performed the grading (e.g., PSA, BGS, CGC)
        /// </summary>
        public string GradingCompany { get; set; }
        
        /// <summary>
        /// The numeric grade assigned to the item (e.g., 9.5, 10)
        /// </summary>
        public decimal Grade { get; set; }
        
        /// <summary>
        /// The unique certification number assigned by the grading company
        /// </summary>
        public string CertificationNumber { get; set; }
        
        /// <summary>
        /// Optional sub-grades for specific aspects (corners, edges, surface, centering)
        /// </summary>
        public string SubGrades { get; set; }
        
        /// <summary>
        /// The date when the item was graded
        /// </summary>
        public DateTime? GradingDate { get; set; }
        
        /// <summary>
        /// Whether the item has been authenticated as genuine
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// Any special notations or qualifiers for the grade (e.g., "OC" for off-center)
        /// </summary>
        public string Qualifiers { get; set; }
    }
} 