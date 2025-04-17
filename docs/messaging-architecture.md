# TCG Order Management System - Messaging Architecture

This document describes the event-driven messaging architecture used in the TCG Order Management System.

## Overview

The TCG Order Management System uses a message-driven architecture to enable loose coupling between microservices. The system utilizes RabbitMQ as the message broker to handle the communication between services. This approach allows for:

- Asynchronous processing
- Resilience (services can continue to function independently)
- Scalability (services can be scaled individually)
- Event sourcing (events serve as the source of truth)

## RabbitMQ Configuration

All services share a common RabbitMQ configuration mechanism, defined in the `TCGOrderManagement.Shared.Messaging.RabbitMqConfig` class. The configuration is loaded from environment variables:

| Environment Variable | Description |
|----------------------|-------------|
| RABBITMQ_HOST        | RabbitMQ server hostname |
| RABBITMQ_PORT        | RabbitMQ server port |
| RABBITMQ_USERNAME    | Username for authentication |
| RABBITMQ_PASSWORD    | Password for authentication |
| RABBITMQ_VHOST       | Virtual host |
| RABBITMQ_EXCHANGE    | Exchange name for publishing events |

The `MessageHandlingExtensions.AddRabbitMqMessaging()` method loads this configuration and registers the appropriate event publisher.

## Topic Exchange

The system uses a RabbitMQ topic exchange, which allows for flexible routing based on message patterns:

- `order.*` - Order-related events
- `inventory.*` - Inventory-related events
- `payment.*` - Payment-related events
- `shipping.*` - Shipping-related events

## Events

### Order Service Events

| Event | Routing Key | Description |
|-------|-------------|-------------|
| OrderCreatedEvent | order.created | Published when a new order is created |
| OrderUpdatedEvent | order.updated | Published when an order is updated |
| PaymentProcessedEvent | order.payment.processed | Published when a payment is processed |
| InventoryReservedEvent | order.inventory.reserved | Published when inventory is reserved |
| InventoryReservationFailedEvent | order.inventory.reservation.failed | Published when inventory reservation fails |
| ShippingRateCalculatedEvent | order.shipping.rate.calculated | Published when shipping rates are calculated |
| OrderCancelledEvent | order.cancelled | Published when an order is cancelled |
| OrderCompletedEvent | order.completed | Published when an order is completed |

### Inventory Service Events

| Event | Routing Key | Description |
|-------|-------------|-------------|
| ItemCreatedEvent | inventory.item.created | Published when a new inventory item is created |
| ItemUpdatedEvent | inventory.item.updated | Published when an inventory item is updated |
| ItemDeletedEvent | inventory.item.deleted | Published when an inventory item is deleted |
| InventoryChangedEvent | inventory.quantity.changed | Published when inventory quantity changes |
| LowInventoryEvent | inventory.quantity.low | Published when inventory is running low |

## Event Publishers

Each service has its own implementation of an `IEventPublisher` interface:

1. `OrderService.Events.RabbitMqEventPublisher` - Publishes order-related events
2. `InventoryService.Events.RabbitMqEventPublisher` - Publishes inventory-related events

These implementations are responsible for:
- Establishing a connection to RabbitMQ
- Creating and configuring the exchange
- Serializing event data to JSON
- Publishing events with appropriate routing keys

## Error Handling and Resilience

The RabbitMQ publishers include error handling to log errors during publishing and connection issues. The publisher implements `IDisposable` to ensure proper cleanup of resources.

## Message Format

Events are serialized to JSON with the following message properties:

- ContentType: application/json
- DeliveryMode: 2 (persistent)
- MessageId: Unique GUID
- Timestamp: UTC timestamp
- Headers:
  - EventType: The C# type name of the event

## Setup and Registration

Services register the messaging components using extension methods:

```csharp
// In Startup.cs
services.AddRabbitMqMessaging(Configuration);
```

This extension method is provided by `TCGOrderManagement.Shared.Extensions.MessageHandlingExtensions`.

## Future Enhancements

Potential enhancements to the messaging architecture include:

1. **Dead Letter Queues** - For handling failed message processing
2. **Message Retry** - Implementing retry policies for transient failures
3. **Message Validation** - Schema validation for messages
4. **Circuit Breakers** - For handling downstream service failures
5. **Message Versioning** - To support evolution of message contracts
6. **Monitoring** - Enhanced monitoring of message flows 