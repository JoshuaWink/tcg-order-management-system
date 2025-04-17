# TCG Order Management API Demo

This repository contains a demonstration of modern C# and .NET backend development practices in a microservice-based architecture. The application showcases implementation of industry standard patterns, security best practices, and modern software development techniques through a RESTful API without frontend components.

## Project Purpose

This API-only demo serves as a portfolio piece to demonstrate:

1. **C# & .NET Backend Expertise**: Showcasing proficiency with .NET Framework 4.0 and C# 7.0 in building robust, scalable APIs
2. **Microservice Architecture Design**: Implementing domain-driven microservices with clear boundaries
3. **Database Integration Skills**: Working with both SQL Server and MongoDB for different data storage needs
4. **API Development Best Practices**: RESTful design, proper status codes, validation, and documentation
5. **Advanced Security Implementation**: Authentication, authorization, and secure data handling
6. **Event-Driven Communication**: Using RabbitMQ for service-to-service communication

The project is specifically designed to highlight backend development capabilities for trading card game (TCG) marketplace systems.

## Project Structure

The portfolio is organized into several backend microservices:

- **OrderService**: Handles order processing and management
- **InventoryService**: Manages product inventory and availability
- **PaymentService**: Processes payments and financial transactions
- **ShippingService**: Manages shipping logistics and fulfillment
- **NotificationService**: Handles user notifications and communication

## Technology Stack

- **.NET Framework 4.0**
- **C# 7.0**
- **SQL Server**: Primary relational database
- **MongoDB**: Document database for inventory data
- **RabbitMQ**: Message broker for service communication
- **Dapper**: Lightweight ORM for data access
- **AutoMapper**: Object-to-object mapping
- **NLog**: Comprehensive logging
- **MSTest**: Unit and integration testing
- **Swagger/OpenAPI**: API documentation and testing interface

## Getting Started

### Prerequisites

- Visual Studio 2019 or newer
- .NET Framework 4.0 SDK
- SQL Server 2019 (Developer Edition)
- MongoDB 4.4+
- RabbitMQ 3.8+

### Running the Services

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Update connection strings in `appsettings.json` files
5. Run database migrations:
   ```
   Update-Database -Project OrderService
   ```
6. Start the services in the following order:
   - RabbitMQ
   - InventoryService
   - PaymentService
   - ShippingService
   - NotificationService
   - OrderService

7. Access the API documentation at `https://localhost:5001/` using Swagger UI

## API Features

- **Order Processing**: End-to-end order workflow via RESTful endpoints
- **Inventory Management**: Real-time tracking and reservation APIs
- **Payment Processing**: Secure payment handling endpoints
- **Shipping Integration**: Seamless shipping options through API
- **Real-time Notifications**: Event-based notification system
- **Comprehensive Security**: Authentication, encryption, and secure coding practices

## Architecture

The application follows a microservice architecture pattern with the following key principles:

- **Domain-Driven Design**: Services organized around business capabilities
- **Event-Driven Communication**: Services communicate through events via message broker
- **CQRS Pattern**: Separation of read and write operations
- **Repository Pattern**: Abstraction of data access logic
- **RESTful API Design**: Following REST best practices for all endpoints

For a detailed overview of the implementation, see [Implementation Overview](docs/implementation-overview.md).

## Security

This API demo demonstrates enterprise-grade security practices:

- JWT-based authentication
- Secure password storage with BCrypt
- Data encryption for sensitive information
- Protection against common web vulnerabilities
- Secure configuration management

## Testing

The application includes:

- **Unit Tests**: Testing individual components in isolation
- **Integration Tests**: Testing interaction between components
- **API Tests**: Testing endpoints and API contracts

## Future Improvements

- Implement API Gateway for unified entry point
- Add containerization with Docker
- Enhance monitoring and observability
- Implement CI/CD pipeline
- Add performance testing and optimization
- Develop a complementary frontend client application

## 🔍 Code Structure

```
tcg-order-management-api/
│
├── Services/
│   ├── OrderService/
│   ├── InventoryService/
│   └── FulfillmentService/
│
├── Shared/
│   ├── EventBus/
│   ├── Logging/
│   ├── Security/
│   └── Utilities/
│
└── Tests/
    ├── OrderService.Tests/
    ├── InventoryService.Tests/
    └── FulfillmentService.Tests/
```

## 📚 Additional Documentation

- [Detailed Architecture](ARCHITECTURE.md)
- [Messaging Architecture](docs/messaging-architecture.md)
- [API Reference](API-REFERENCE.md)
- [Security Implementation](docs/security-implementation.md)
- [Security Validation Results](docs/security-validation-results.md)
- [Code Quality Validation](docs/code-quality-validation.md)
- [Project Assessment](docs/project-assessment.md)

---

**Note**: This is a portfolio demonstration project created to showcase backend API development skills and system design capabilities. While it implements core functionality, it is not intended for production use without additional development considerations. The project deliberately focuses on API development without frontend components to highlight backend expertise. 
