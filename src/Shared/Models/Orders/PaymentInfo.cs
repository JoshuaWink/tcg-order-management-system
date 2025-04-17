using System;

namespace TCGOrderManagement.Shared.Models.Orders
{
    /// <summary>
    /// Represents payment information for an order
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>
        /// Unique identifier for the payment record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Reference to the order
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Payment method (e.g., Credit Card, PayPal, Store Credit)
        /// </summary>
        public PaymentMethod Method { get; set; }
        
        /// <summary>
        /// Status of the payment
        /// </summary>
        public PaymentStatus Status { get; set; }
        
        /// <summary>
        /// Transaction identifier from the payment processor
        /// </summary>
        public string TransactionId { get; set; }
        
        /// <summary>
        /// When the payment was processed
        /// </summary>
        public DateTime? ProcessedDate { get; set; }
        
        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Currency of the payment (e.g., USD, EUR)
        /// </summary>
        public string Currency { get; set; }
        
        /// <summary>
        /// Last 4 digits of the credit card (if applicable)
        /// </summary>
        public string Last4Digits { get; set; }
        
        /// <summary>
        /// Card type (if applicable)
        /// </summary>
        public string CardType { get; set; }
        
        /// <summary>
        /// Whether the payment was authorized
        /// </summary>
        public bool IsAuthorized { get; set; }
        
        /// <summary>
        /// Whether the payment was captured
        /// </summary>
        public bool IsCaptured { get; set; }
        
        /// <summary>
        /// Authorization code from the payment processor
        /// </summary>
        public string AuthorizationCode { get; set; }
        
        /// <summary>
        /// Error message if payment failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Billing address associated with the payment
        /// </summary>
        public string BillingAddress { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PaymentInfo()
        {
            Id = Guid.NewGuid();
            Currency = "USD"; // Default currency
            Status = PaymentStatus.Pending;
        }
    }
    
    /// <summary>
    /// Enumeration of payment methods
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Credit or debit card
        /// </summary>
        CreditCard = 0,
        
        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 1,
        
        /// <summary>
        /// Store credit
        /// </summary>
        StoreCredit = 2,
        
        /// <summary>
        /// Gift card
        /// </summary>
        GiftCard = 3,
        
        /// <summary>
        /// Bank transfer
        /// </summary>
        BankTransfer = 4,
        
        /// <summary>
        /// Cash on delivery
        /// </summary>
        CashOnDelivery = 5,
        
        /// <summary>
        /// Other payment method
        /// </summary>
        Other = 6
    }
    
    /// <summary>
    /// Enumeration of payment statuses
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment is pending
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Payment is authorized but not captured
        /// </summary>
        Authorized = 1,
        
        /// <summary>
        /// Payment is completed successfully
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Payment failed
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// Payment was refunded
        /// </summary>
        Refunded = 4,
        
        /// <summary>
        /// Payment was partially refunded
        /// </summary>
        PartiallyRefunded = 5,
        
        /// <summary>
        /// Payment was voided
        /// </summary>
        Voided = 6,
        
        /// <summary>
        /// Payment is in dispute
        /// </summary>
        Disputed = 7
    }
} 