using System;

namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents shipping information for an order
    /// </summary>
    public class ShippingInfo
    {
        /// <summary>
        /// Unique identifier for the shipping record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Reference to the order
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Recipient's full name
        /// </summary>
        public string RecipientName { get; set; }
        
        /// <summary>
        /// First line of the address
        /// </summary>
        public string AddressLine1 { get; set; }
        
        /// <summary>
        /// Second line of the address (optional)
        /// </summary>
        public string AddressLine2 { get; set; }
        
        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// State or province
        /// </summary>
        public string StateProvince { get; set; }
        
        /// <summary>
        /// Postal or ZIP code
        /// </summary>
        public string PostalCode { get; set; }
        
        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }
        
        /// <summary>
        /// Phone number for delivery questions
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Special delivery instructions
        /// </summary>
        public string DeliveryInstructions { get; set; }
        
        /// <summary>
        /// Shipping method (e.g., Standard, Express, Overnight)
        /// </summary>
        public string ShippingMethod { get; set; }
        
        /// <summary>
        /// Shipping carrier (e.g., USPS, FedEx, UPS)
        /// </summary>
        public string Carrier { get; set; }
        
        /// <summary>
        /// Tracking number
        /// </summary>
        public string TrackingNumber { get; set; }
        
        /// <summary>
        /// Estimated delivery date
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        /// <summary>
        /// Actual shipping date
        /// </summary>
        public DateTime? ShippedDate { get; set; }
        
        /// <summary>
        /// Actual delivery date
        /// </summary>
        public DateTime? DeliveredDate { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ShippingInfo()
        {
            Id = Guid.NewGuid();
        }
    }
} 