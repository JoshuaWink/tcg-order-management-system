# TCG API Portfolio Implementation Overview

This document provides a technical overview of the implementation of the TCG API Portfolio project, focusing on the architectural decisions, design patterns, and backend technologies used in this API-only demonstration.

## Backend Architecture

The project implements a microservice architecture with the following backend services:

### Core API Services

1. **API Gateway** - Entry point for client applications (e.g., potential frontend clients, mobile apps, or third-party integrations)
2. **Order Service** - Manages orders and payment processing
3. **Inventory Service** - Handles product inventory management
4. **Payment Service** - Processes payment transactions
5. **Shipping Service** - Manages shipping and delivery
6. **Notification Service** - Handles user notifications

This API-only implementation deliberately focuses on backend development skills without including frontend components, allowing for a clear demonstration of API design, microservice architecture, and backend integration patterns.

### Communication Patterns

The services communicate using:

- **Synchronous communication** via REST APIs for immediate responses
- **Asynchronous communication** via message queues (RabbitMQ) for event-driven operations

## Design Patterns

### Repository Pattern

The application implements the repository pattern to abstract data access:

```csharp
public interface IInventoryRepository
{
    Task<InventoryItem> GetInventoryAsync(string itemId);
    Task<bool> ReserveInventoryAsync(string itemId, int quantity, string orderId);
    Task<bool> ConfirmReservationAsync(string reservationId);
    Task<bool> ReleaseReservationAsync(string reservationId);
}
```

This pattern:
- Decouples business logic from data access
- Enables unit testing through mocking
- Allows for different database implementations without changing domain logic

### CQRS Pattern

The Command Query Responsibility Segregation pattern separates read and write operations:

```csharp
// Command
public class CreateOrderCommand
{
    public string UserId { get; set; }
    public List<OrderItem> Items { get; set; }
    public ShippingDetails ShippingDetails { get; set; }
}

// Query
public class GetOrderByIdQuery
{
    public string OrderId { get; set; }
}
```

Benefits include:
- Scalability through separate optimization of read and write paths
- Simplified models for specific operations
- Improved performance for read-heavy workloads

### Event-Driven Architecture

The system uses events to communicate state changes between services:

```csharp
public class OrderCreatedEvent
{
    public string OrderId { get; }
    public string UserId { get; }
    public DateTime CreatedDate { get; }
    public List<OrderItem> Items { get; }
    
    // Constructor and methods
}
```

This enables:
- Loose coupling between services
- Better resilience to service failures
- Natural audit trail of system activities

## Data Access

### Multiple Database Types

The project demonstrates working with different database technologies:

1. **SQL Server** - For structured, relational data (Orders, Users)
2. **MongoDB** - For document storage (Inventory Items)

### Sample Repository Implementations

#### SQL Repository Example:

```csharp
public async Task<Order> GetOrderByIdAsync(string orderId)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        await connection.OpenAsync();
        
        var command = new SqlCommand(
            "SELECT * FROM Orders WHERE Id = @OrderId", 
            connection);
        
        command.Parameters.Add("@OrderId", SqlDbType.NVarChar).Value = orderId;
        
        using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                return new Order
                {
                    Id = reader.GetString(reader.GetOrdinal("Id")),
                    UserId = reader.GetString(reader.GetOrdinal("UserId")),
                    Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                    // Additional property mapping
                };
            }
            
            return null;
        }
    }
}
```

#### MongoDB Repository Example:

```csharp
public async Task<InventoryItem> GetInventoryAsync(string itemId)
{
    return await _inventoryCollection
        .Find(item => item.Id == itemId)
        .FirstOrDefaultAsync();
}
```

## Security Implementation

The project implements several security features, detailed in the [Security Implementation](security-implementation.md) document:

1. Secure authentication with JWT
2. Password hashing with BCrypt
3. Data encryption for sensitive fields
4. Input validation and parameterized queries
5. Secure configuration management

## API Implementation

### RESTful API Design

The API follows REST principles:

- Uses appropriate HTTP methods (GET, POST, PUT, DELETE)
- Implements proper status codes
- Provides resource-based URLs
- Includes pagination for collection resources

### Sample Controller:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(string id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
            return NotFound();
            
        return Ok(MapToDto(order));
    }
    
    // Additional endpoints
}
```

## Event Processing

### Publisher Implementation

The project includes a robust event publishing mechanism:

```csharp
public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    
    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(
            exchange: "tcg_events", 
            type: ExchangeType.Topic);
    }
    
    public async Task PublishEventAsync<T>(T @event, string routingKey)
    {
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: "tcg_events",
            routingKey: routingKey,
            basicProperties: null,
            body: body);
        
        _logger.LogInformation($"Published event {typeof(T).Name} with routing key {routingKey}");
        
        await Task.CompletedTask;
    }
}
```

### Consumer Implementation

Event consumers handle the received events:

```csharp
public class OrderEventConsumer : IEventConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderEventConsumer> _logger;
    
    public OrderEventConsumer(
        IOrderService orderService,
        ILogger<OrderEventConsumer> logger,
        IConfiguration configuration)
    {
        _orderService = orderService;
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(
            exchange: "tcg_events", 
            type: ExchangeType.Topic);
            
        var queueName = _channel.QueueDeclare().QueueName;
        
        _channel.QueueBind(
            queue: queueName,
            exchange: "tcg_events",
            routingKey: "inventory.#");
            
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += ProcessMessage;
        
        _channel.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: consumer);
    }
    
    private void ProcessMessage(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var routingKey = ea.RoutingKey;
        
        _logger.LogInformation($"Received message with routing key {routingKey}");
        
        // Process different event types based on routing key
        if (routingKey == "inventory.reserved")
        {
            var event = JsonConvert.DeserializeObject<InventoryReservedEvent>(message);
            _orderService.ProcessInventoryReservedAsync(event);
        }
        
        // Handle other event types
    }
}
```

## Conclusion

This API-only portfolio project demonstrates modern software architecture patterns focused on backend development. Key highlights include:

1. **Microservice Architecture**: Clear service boundaries with dedicated responsibilities
2. **Event-Driven Communication**: Loose coupling through message-based integration
3. **Domain-Driven Design**: Business concepts modeled in code with appropriate abstractions
4. **Multiple Database Technologies**: Working with both relational and NoSQL databases
5. **RESTful API Design**: Following industry best practices for API development
6. **Security Implementation**: Authentication, authorization, and data protection

The project deliberately focuses on backend API development skills without including frontend components, providing a clear demonstration of server-side architecture, patterns, and implementation techniques that would integrate with any client technology stack. 