using System;
using System.Collections.Generic;
using TCGOrderManagement.Shared.Models.Orders;
using TCGOrderManagement.Shared.Models.Items;
using TCGOrderManagement.Shared.Models.Shipping;
using TCGOrderManagement.OrderService.Models;
using TCGOrderManagement.Shared.Events;

namespace TCGOrderManagement.OrderService.Events
{
    /// <summary>
    /// Base class for all order-related events
    /// </summary>
    public abstract class OrderEvent
    {
        /// <summary>
        /// Gets the unique identifier of the order
        /// </summary>
        public Guid OrderId { get; }

        /// <summary>
        /// Gets the timestamp when the event was created
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        protected OrderEvent(Guid orderId)
        {
            OrderId = orderId;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Event published when a new order is created
    /// </summary>
    public class OrderCreatedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the customer ID who placed the order
        /// </summary>
        public Guid CustomerId { get; }
        
        /// <summary>
        /// Gets the total amount of the order
        /// </summary>
        public decimal TotalAmount { get; }
        
        /// <summary>
        /// Gets the number of items in the order
        /// </summary>
        public int ItemCount { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCreatedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="totalAmount">The total amount</param>
        /// <param name="itemCount">The item count</param>
        public OrderCreatedEvent(Guid orderId, Guid customerId, decimal totalAmount, int itemCount)
            : base(orderId)
        {
            CustomerId = customerId;
            TotalAmount = totalAmount;
            ItemCount = itemCount;
        }
    }

    /// <summary>
    /// Event published when an order's status changes
    /// </summary>
    public class OrderStatusChangedEvent : OrderEvent
    {
        /// <summary>
        /// The previous status of the order
        /// </summary>
        public OrderStatus PreviousStatus { get; set; }
        
        /// <summary>
        /// The new status of the order
        /// </summary>
        public OrderStatus NewStatus { get; set; }
        
        /// <summary>
        /// Optional reason for the status change
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderStatusChangedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderStatusChangedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event raised when a payment has been processed
    /// </summary>
    public class PaymentProcessedEvent : OrderEvent
    {
        /// <summary>
        /// Gets a value indicating whether the payment was successful
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the payment method used
        /// </summary>
        public string PaymentMethod { get; }
        
        /// <summary>
        /// Gets the payment transaction reference
        /// </summary>
        public string TransactionReference { get; }
        
        /// <summary>
        /// Gets the reason for payment failure, if applicable
        /// </summary>
        public string FailureReason { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentProcessedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="isSuccess">Indication if payment was successful</param>
        /// <param name="paymentMethod">The payment method used</param>
        /// <param name="transactionReference">The payment transaction reference</param>
        /// <param name="failureReason">The reason for payment failure, if applicable</param>
        public PaymentProcessedEvent(Guid orderId, bool isSuccess, string paymentMethod, string transactionReference, string failureReason = null)
            : base(orderId)
        {
            IsSuccess = isSuccess;
            PaymentMethod = paymentMethod;
            TransactionReference = transactionReference;
            FailureReason = failureReason;
        }
    }

    /// <summary>
    /// Event published when shipping information for an order is updated
    /// </summary>
    public class OrderShippingUpdatedEvent : OrderEvent
    {
        /// <summary>
        /// The shipping address
        /// </summary>
        public string ShippingAddress { get; set; }
        
        /// <summary>
        /// The shipping method used
        /// </summary>
        public string ShippingMethod { get; set; }
        
        /// <summary>
        /// The estimated delivery date
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        /// <summary>
        /// The shipping cost
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderShippingUpdatedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderShippingUpdatedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an order is shipped
    /// </summary>
    public class OrderShippedEvent : OrderEvent
    {
        /// <summary>
        /// The tracking number for the shipment
        /// </summary>
        public string TrackingNumber { get; set; }
        
        /// <summary>
        /// The carrier handling the shipment
        /// </summary>
        public string Carrier { get; set; }
        
        /// <summary>
        /// The date the order was shipped
        /// </summary>
        public DateTime ShippedDate { get; set; }
        
        /// <summary>
        /// The estimated delivery date
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderShippedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderShippedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an order is cancelled
    /// </summary>
    public class OrderCancelledEvent : OrderEvent
    {
        /// <summary>
        /// The reason for cancellation
        /// </summary>
        public string CancellationReason { get; set; }
        
        /// <summary>
        /// Whether a refund is being issued
        /// </summary>
        public bool IsRefundIssued { get; set; }
        
        /// <summary>
        /// The amount being refunded
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCancelledEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderCancelledEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an item is added to an order
    /// </summary>
    public class OrderItemAddedEvent : OrderEvent
    {
        /// <summary>
        /// The unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// The quantity of the item added
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The unit price of the item
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// The new order total after adding the item
        /// </summary>
        public decimal NewOrderTotal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderItemAddedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderItemAddedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an item is removed from an order
    /// </summary>
    public class OrderItemRemovedEvent : OrderEvent
    {
        /// <summary>
        /// The unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// The quantity of the item removed
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The new order total after removing the item
        /// </summary>
        public decimal NewOrderTotal { get; set; }
        
        /// <summary>
        /// The reason for removing the item
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderItemRemovedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderItemRemovedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an order item's quantity is updated
    /// </summary>
    public class OrderItemQuantityUpdatedEvent : OrderEvent
    {
        /// <summary>
        /// The unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// The previous quantity of the item
        /// </summary>
        public int PreviousQuantity { get; set; }
        
        /// <summary>
        /// The new quantity of the item
        /// </summary>
        public int NewQuantity { get; set; }
        
        /// <summary>
        /// The new order total after updating the quantity
        /// </summary>
        public decimal NewOrderTotal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderItemQuantityUpdatedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderItemQuantityUpdatedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event published when an order return is processed
    /// </summary>
    public class OrderReturnProcessedEvent : OrderEvent
    {
        /// <summary>
        /// The unique identifier for the return
        /// </summary>
        public Guid ReturnId { get; set; }
        
        /// <summary>
        /// The items being returned with their quantities
        /// </summary>
        public ReturnedItem[] ReturnedItems { get; set; }
        
        /// <summary>
        /// The reason for the return
        /// </summary>
        public string ReturnReason { get; set; }
        
        /// <summary>
        /// The amount being refunded
        /// </summary>
        public decimal RefundAmount { get; set; }
        
        /// <summary>
        /// Whether the return was approved
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderReturnProcessedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderReturnProcessedEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Represents an item that is being returned
    /// </summary>
    public class ReturnedItem
    {
        /// <summary>
        /// The unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }
        
        /// <summary>
        /// The quantity being returned
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The condition of the item upon return
        /// </summary>
        public string Condition { get; set; }
    }

    /// <summary>
    /// Event published when an order is delivered
    /// </summary>
    public class OrderDeliveredEvent : OrderEvent
    {
        /// <summary>
        /// The date the order was delivered
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        
        /// <summary>
        /// Optional signature confirmation
        /// </summary>
        public string SignatureConfirmation { get; set; }
        
        /// <summary>
        /// Any notes about the delivery
        /// </summary>
        public string DeliveryNotes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDeliveredEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        public OrderDeliveredEvent(Guid orderId)
            : base(orderId)
        {
        }
    }

    /// <summary>
    /// Event raised when inventory has been reserved for an order
    /// </summary>
    public class InventoryReservedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the date when the reservation expires
        /// </summary>
        public DateTimeOffset ReservationExpiryDate { get; }

        /// <summary>
        /// Gets the list of items that were successfully reserved
        /// </summary>
        public List<ReservedItem> ReservedItems { get; set; } = new List<ReservedItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryReservedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="reservationExpiryDate">The date when the reservation expires</param>
        public InventoryReservedEvent(Guid orderId, DateTimeOffset reservationExpiryDate)
            : base(orderId)
        {
            ReservationExpiryDate = reservationExpiryDate;
        }
    }

    /// <summary>
    /// Represents an item that was reserved in inventory
    /// </summary>
    public class ReservedItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the item
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity that was reserved
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Event raised when inventory reservation has failed for an order
    /// </summary>
    public class InventoryReservationFailedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the reason for the inventory reservation failure
        /// </summary>
        public string FailureReason { get; }

        /// <summary>
        /// Gets the list of unavailable items that caused the reservation failure
        /// </summary>
        public List<UnavailableItem> UnavailableItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryReservationFailedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="failureReason">The reason for the failure</param>
        /// <param name="unavailableItems">The list of unavailable items</param>
        public InventoryReservationFailedEvent(Guid orderId, string failureReason, List<UnavailableItem> unavailableItems = null)
            : base(orderId)
        {
            FailureReason = failureReason;
            UnavailableItems = unavailableItems ?? new List<UnavailableItem>();
        }
    }

    /// <summary>
    /// Represents an unavailable item in inventory
    /// </summary>
    public class UnavailableItem
    {
        /// <summary>
        /// Gets the unique identifier of the item
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// Gets the requested quantity
        /// </summary>
        public int RequestedQuantity { get; }

        /// <summary>
        /// Gets the available quantity
        /// </summary>
        public int AvailableQuantity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnavailableItem"/> class
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="requestedQuantity">The requested quantity</param>
        /// <param name="availableQuantity">The available quantity</param>
        public UnavailableItem(Guid itemId, int requestedQuantity, int availableQuantity)
        {
            ItemId = itemId;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }

    /// <summary>
    /// Event raised when shipping rates have been calculated for an order
    /// </summary>
    public class ShippingRateCalculatedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the calculated shipping cost
        /// </summary>
        public decimal ShippingCost { get; }

        /// <summary>
        /// Gets the shipping method
        /// </summary>
        public string ShippingMethod { get; }

        /// <summary>
        /// Gets the estimated delivery date
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; }

        /// <summary>
        /// Gets the tracking number for the shipment
        /// </summary>
        public string TrackingNumber { get; }

        /// <summary>
        /// Gets the carrier for the shipment
        /// </summary>
        public string Carrier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShippingRateCalculatedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="shippingCost">The shipping cost</param>
        /// <param name="shippingMethod">The shipping method</param>
        /// <param name="estimatedDeliveryDate">The estimated delivery date</param>
        /// <param name="trackingNumber">The tracking number (optional)</param>
        /// <param name="carrier">The carrier (optional)</param>
        public ShippingRateCalculatedEvent(
            Guid orderId, 
            decimal shippingCost, 
            string shippingMethod, 
            DateTime? estimatedDeliveryDate = null,
            string trackingNumber = null,
            string carrier = null)
            : base(orderId)
        {
            ShippingCost = shippingCost;
            ShippingMethod = shippingMethod;
            EstimatedDeliveryDate = estimatedDeliveryDate;
            TrackingNumber = trackingNumber;
            Carrier = carrier;
        }
    }

    /// <summary>
    /// Event published when an order is updated
    /// </summary>
    public class OrderUpdatedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the updated total amount of the order
        /// </summary>
        public decimal TotalAmount { get; }
        
        /// <summary>
        /// Gets the updated item count
        /// </summary>
        public int ItemCount { get; }
        
        /// <summary>
        /// Gets the reason for the update
        /// </summary>
        public string UpdateReason { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderUpdatedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="totalAmount">The total amount</param>
        /// <param name="itemCount">The item count</param>
        /// <param name="updateReason">The reason for the update</param>
        public OrderUpdatedEvent(Guid orderId, decimal totalAmount, int itemCount, string updateReason)
            : base(orderId)
        {
            TotalAmount = totalAmount;
            ItemCount = itemCount;
            UpdateReason = updateReason;
        }
    }

    /// <summary>
    /// Event published when an order is completed
    /// </summary>
    public class OrderCompletedEvent : OrderEvent
    {
        /// <summary>
        /// Gets the completion date
        /// </summary>
        public DateTimeOffset CompletionDate { get; }
        
        /// <summary>
        /// Gets the total amount of the order
        /// </summary>
        public decimal TotalAmount { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCompletedEvent"/> class
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="completionDate">The completion date</param>
        /// <param name="totalAmount">The total amount</param>
        public OrderCompletedEvent(Guid orderId, DateTimeOffset completionDate, decimal totalAmount)
            : base(orderId)
        {
            CompletionDate = completionDate;
            TotalAmount = totalAmount;
        }
    }
} 