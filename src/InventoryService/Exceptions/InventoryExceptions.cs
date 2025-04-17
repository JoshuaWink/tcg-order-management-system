using System;

namespace TCGOrderManagement.InventoryService.Exceptions
{
    /// <summary>
    /// Exception thrown when an item is not found
    /// </summary>
    public class ItemNotFoundException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemNotFoundException() : base("The requested item was not found.")
        {
        }
        
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message">Exception message</param>
        public ItemNotFoundException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public ItemNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Exception thrown when attempting to add a duplicate item
    /// </summary>
    public class DuplicateItemException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DuplicateItemException() : base("An item with the same identifier already exists.")
        {
        }
        
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message">Exception message</param>
        public DuplicateItemException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public DuplicateItemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Exception thrown when inventory quantity is insufficient for an operation
    /// </summary>
    public class InsufficientInventoryException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InsufficientInventoryException() : base("The inventory quantity is insufficient for this operation.")
        {
        }
        
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message">Exception message</param>
        public InsufficientInventoryException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public InsufficientInventoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Exception thrown when an inventory operation fails
    /// </summary>
    public class InventoryOperationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InventoryOperationException() : base("An error occurred during the inventory operation.")
        {
        }
        
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message">Exception message</param>
        public InventoryOperationException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public InventoryOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 