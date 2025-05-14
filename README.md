# LTYape Microservices

Microservices-based application for processing financial transactions with fraud validation capabilities.

## Architecture

This solution consists of two main microservices:

1. **Transaction Service**: Handles transaction creation and management
2. **Anti-Fraud Service**: Validates transactions against fraud rules

The services communicate through Kafka messaging for asynchronous processing. Each service has its own database for storing domain-specific data.

### Hexagonal Architecture (Ports and Adapters)

The solution implements the Hexagonal Architecture (also known as Ports and Adapters) pattern, which:

- Places the domain model at the center
- Defines ports (interfaces) through which the domain interacts with external systems
- Implements adapters for specific technologies that satisfy these ports
- Isolates business logic from infrastructure concerns

#### Key Components:

1. **Domain**: Contains entities, value objects, and domain services that implement core business logic
2. **Ports**: Interfaces that define how the domain interacts with the outside world
   - **Inbound Ports**: Define operations that can be performed by the outside world on the domain
   - **Outbound Ports**: Define operations that the domain performs on external systems
3. **Adapters**: Implementations of ports using specific technologies
   - **Primary Adapters**: Convert external requests into calls to the domain (e.g., API controllers)
   - **Secondary Adapters**: Connect the domain to external systems (e.g., databases, messaging)

#### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                  │
│  ┌─────────────────────────────────────┐                ┌─────────────────────────────────────┐  │
│  │       TransactionService            │                │         AntiFraudService            │  │
│  │                                     │                │                                     │  │
│  │  ┌─────────────────────────┐        │                │  ┌─────────────────────────┐        │  │
│  │  │     API Layer           │        │                │  │     API Layer           │        │  │
│  │  │  ┌──────────────────┐   │        │                │  │  ┌──────────────────┐   │        │  │
│  │  │  │ Primary Adapters │   │        │                │  │  │ Primary Adapters │   │        │  │
│  │  │  │  (Controllers)   │   │        │                │  │  │  (Controllers)   │   │        │  │
│  │  │  └────────┬─────────┘   │        │                │  │  └────────┬─────────┘   │        │  │
│  │  └───────────┼─────────────┘        │                │  └───────────┼─────────────┘        │  │
│  │              │                      │                │              │                      │  │
│  │  ┌───────────▼────────────┐         │                │  ┌───────────▼────────────┐         │  │
│  │  │   Application Layer    │         │                │  │   Application Layer    │         │  │
│  │  │  ┌─────────────────┐   │         │                │  │  ┌─────────────────┐   │         │  │
│  │  │  │   Application   │   │         │                │  │  │   Application   │   │         │  │
│  │  │  │    Services     │   │         │                │  │  │    Services     │   │         │  │
│  │  │  └────────┬────────┘   │         │                │  │  └────────┬────────┘   │         │  │
│  │  └───────────┼────────────┘         │                │  └───────────┼────────────┘         │  │
│  │              │                      │                │              │                      │  │
│  │  ┌───────────▼────────────┐         │                │  ┌───────────▼────────────┐         │  │
│  │  │     Domain Layer       │         │                │  │     Domain Layer       │         │  │
│  │  │  ┌─────────────────┐   │         │                │  │  ┌─────────────────┐   │         │  │
│  │  │  │  Domain Model   │   │         │                │  │  │  Domain Model   │   │         │  │
│  │  │  │    Entities     │   │         │                │  │  │    Entities     │   │         │  │
│  │  │  │   Services      │   │         │                │  │  │   Services      │   │         │  │
│  │  │  └────────┬────────┘   │         │                │  │  └────────┬────────┘   │         │  │
│  │  │           │            │         │                │  │           │            │         │  │
│  │  │  ┌────────▼────────┐   │         │                │  │  ┌────────▼────────┐   │         │  │
│  │  │  │      Ports      │   │         │                │  │  │      Ports      │   │         │  │
│  │  │  │ ┌──────────────┐│   │         │                │  │  │ ┌──────────────┐│   │         │  │
│  │  │  │ │ITransactionRe││   │         │                │  │  │ │ITransactionVa││   │         │  │
│  │  │  │ │positoryPort  ││   │         │                │  │  │ │lidationRepo- ││   │         │  │
│  │  │  │ └──────────────┘│   │         │                │  │  │ │sitoryPort    ││   │         │  │
│  │  │  │ ┌──────────────┐│   │         │                │  │  │ └──────────────┘│   │         │  │
│  │  │  │ │IAntiFraudEven││   │         │                │  │  │ ┌──────────────┐│   │         │  │
│  │  │  │ │tPort         ││   │         │                │  │  │ │ITransactionEv││   │         │  │
│  │  │  │ └──────────────┘│   │         │                │  │  │ │entPort       ││   │         │  │
│  │  │  └────────┬────────┘   │         │                │  │  │ └──────────────┘│   │         │  │
│  │  └───────────┼────────────┘         │                │  │  └────────┬────────┘   │         │  │
│  │              │                      │                │  └───────────┼────────────┘         │  │
│  │  ┌───────────▼────────────┐         │                │  ┌───────────▼─────────────┐        │  │
│  │  │  Infrastructure Layer  │         │                │  │  Infrastructure Layer   │        │  │
│  │  │  ┌──────────────────┐  │         │                │  │  ┌──────────────────┐   │        │  │
│  │  │  │Secondary Adapters│  │         │                │  │  │Secondary Adapters│   │        │  │
│  │  │  │ ┌──────────────┐ │  │         │                │  │  │ ┌──────────────┐ │   │        │  │
│  │  │  │ │TransactionRep│ │  │         │                │  │  │ │TransactionVal│ │   │        │  │
│  │  │  │ │ositoryAdapter│ │  │         │                │  │  │ │idationReposi-│ │   │        │  │
│  │  │  │ └──────────────┘ │  │         │                │  │  │ │toryAdapter   │ │   │        │  │
│  │  │  │ ┌──────────────┐ │  │         │                │  │  │ └──────────────┘ │   │        │  │
│  │  │  │ │KafkaAntiFraud│ │  │         │                │  │  │ ┌───────────────┐│   │        │  │
│  │  │  │ │Service       │ │  │         │                │  │  │ │KafkaTransacti ││   │        │  │
│  │  │  │ └──────────────┘ │  │         │                │  │  │ │onEventService ││   │        │  │
│  │  │  └─────────┬─────── ┘  │         │                │  │  │ └───────────────┘│   │        │  │
│  │  └────────────┼───────────┘         │                │  │  └─────────┬────────┘   │        │  │
│  │               │                     │                │  └────────────┼────────────┘        │  │
│  │               │                     │                │               │                     │  │
│  │               │                     │                │               │                     │  │
│  │               ▼                     │                │               ▼                     │  │
│  │    ◄────────────── Kafka Messaging ─────────────────────────────────────────────────►      │  │
│  │                                     │                │                                     │  │
│  │  ┌────────────▼──────────┐          │                │  ┌────────────▼──────────┐          │  │
│  │  │     SQL Database      │          │                │  │     SQL Database      │          │  │
│  │  │                       │          │                │  │                       │          │  │
│  │  │    TransactionDb      │          │                │  │     AntiFraudDb       │          │  │
│  │  └───────────────────────┘          │                │  └───────────────────────┘          │  │
│  └─────────────────────────────────────┘                └─────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Benefits of Hexagonal Architecture

1. **Domain Independence**: Domain logic doesn't depend on infrastructure.
2. **Testability**: Easy to test business rules in isolation.
3. **Flexibility**: Freedom to swap infrastructure components without changing business logic.
4. **Separation of Concerns**: Each component has a single responsibility.

### Communication Flow

1. **Transaction Service** receives a request to create a transaction.
2. The transaction is saved in "pending" state in the database (SQL Server).
3. A message is sent through Kafka to be validated by **Anti-Fraud Service**.
4. **Anti-Fraud Service** receives the message, queries historical information in SQL Server, and validates the transaction according to fraud rules.
5. **Anti-Fraud Service** sends a response message through Kafka.
6. **Transaction Service** receives the response and updates the transaction state to "approved" or "rejected" in SQL Server.

## Validation Rules

Transactions are rejected when:
- The transaction value is greater than 2000.
- The daily accumulated amount is greater than 20000.

## Services URLs

When running with Docker Compose, the services are available at:

- **Transaction Service**: http://localhost:5001
- **Anti-Fraud Service**: http://localhost:5002

## API Endpoints

### Transaction Service

#### Create Transaction
- **URL**: `POST http://localhost:5001/api/transactions/set`
- **Description**: Creates a new financial transaction
- **Request Body**: JSON with transaction details
- **Response**: Transaction details with a 201 Created status

#### Get Transaction
- **URL**: `POST http://localhost:5001/api/transactions/get`
- **Description**: Retrieves details of a specific transaction
- **Request Body**: 
```json
{
    "transactionExternalId": "00000000-0000-0000-0000-000000000000",
    "createdAt": "2024-03-21T10:00:00Z"  // opcional
}
```
- **Response**: Transaction details with a 200 OK status or 404 Not Found

### Anti-Fraud Service

#### Validate Transaction (Manual validation)
- **URL**: `POST http://localhost:5002/api/antifraud/validate`
- **Description**: Manually validates a transaction (for testing purposes)
- **Request Body**: JSON with transaction details
- **Response**: 204 No Content if successful

## Transaction Examples

### Example 1: Standard Transaction (Small Amount)
```json
{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "4ee0950e-7ee6-4542-9930-6bc6e7a2ce8c",
  "transferTypeId": 1,
  "value": 100.00
}
```

### Example 2: Large Amount Transaction (Will be rejected)
```json
{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "4ee0950e-7ee6-4542-9930-6bc6e7a2ce8c",
  "transferTypeId": 1,
  "value": 12000.00
}
```

### Example 3: Multiple Transactions (To test daily limit)
```json
{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "4ee0950e-7ee6-4542-9930-6bc6e7a2ce8c",
  "transferTypeId": 1,
  "value": 3000.00
}
```

### Example 4: ATM Withdrawal
```json
{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "4ee0950e-7ee6-4542-9930-6bc6e7a2ce8c",
  "transferTypeId": 2,
  "value": 200.00
}
```

### Example 5: International Transfer
```json
{
  "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetAccountId": "4ee0950e-7ee6-4542-9930-6bc6e7a2ce8c",
  "transferTypeId": 3,
  "value": 500.00
}
```

## Transfer Types

The `transferTypeId` field specifies the type of transfer:

- `1`: Standard bank transfer
- `2`: ATM withdrawal
- `3`: International transfer

## Local Development with Docker

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for local development outside Docker)

### Running the Application

1. Build and start the containers:
   ```
   docker-compose up -d --build
   ```

2. The services will be available at:
   - Transaction Service: http://localhost:5001
   - Anti-Fraud Service: http://localhost:5002
   - Swagger UI: http://localhost:5001/swagger and http://localhost:5002/swagger

3. Stop the application:
   ```
   docker-compose down
   ```

## Project Structure

The solution follows a hexagonal architecture approach:

### TransactionService
- **API Layer**: Controllers (primary adapters) that receive external requests
- **Application Layer**: Application services that orchestrate use cases
- **Domain Layer**: 
  - **Core**: Entities, value objects, domain services that encapsulate business rules
  - **Ports**: Interfaces defining how the domain interacts with external systems
- **Infrastructure Layer**: Secondary adapters implementing ports for specific technologies
  - Database repositories
  - Kafka messaging
  - External service clients

### AntiFraudService
- **API Layer**: Controllers (primary adapters)
- **Application Layer**: Application services for fraud validation
- **Domain Layer**: 
  - **Core**: Validation rules, entities, and domain services
  - **Ports**: Repository and messaging interfaces
- **Infrastructure Layer**: Concrete implementations for database access and Kafka communication

## Key Ports and Adapters

### TransactionService
- **Ports** (interfaces):
  - `ITransactionRepositoryPort`: Interface for transaction persistence operations
  - `IAntiFraudEventPort`: Interface for sending transactions to Anti-Fraud service

- **Adapters** (implementations):
  - `TransactionRepositoryAdapter`: EF Core implementation of the transaction repository port
  - `KafkaAntiFraudService`: Kafka implementation of the anti-fraud event port

### AntiFraudService
- **Ports** (interfaces):
  - `ITransactionEventPort`: Interface for sending validation responses
  - `ITransactionEventConsumerPort`: Interface for consuming transaction events
  - `ITransactionValidationRepositoryPort`: Interface for validation repository operations

- **Adapters** (implementations):
  - `KafkaTransactionEventService`: Kafka implementation for sending validation responses
  - `KafkaTransactionEventConsumerService`: Kafka implementation for consuming transaction events
  - `TransactionValidationRepositoryAdapter`: EF Core implementation of the validation repository port

## Database Connection

### SQL Server Details
- **Server**: localhost,1433
- **User**: sa
- **Password**: Pass@word1
- **Databases**: 
  - Transaction Service: TransactionDb
  - Anti-Fraud Service: AntiFraudDb

### Using SQL Server Management Studio
1. Connect using the following settings:
   - Server type: Database Engine
   - Server name: localhost,1433
   - Authentication: SQL Server Authentication
   - Login: sa
   - Password: Pass@word1

## Technologies

- **.NET 8**: Framework for application development
- **SQL Server**: Relational database
- **Kafka**: Messaging system for microservices communication
- **Docker/Docker Compose**: For containerization and service orchestration
- **Entity Framework Core**: For database access

## Troubleshooting

### Kafka Connection Issues
If you encounter issues connecting to Kafka, check:
1. Ensure the Kafka container is running: `docker ps | grep kafka`
2. Verify the Kafka topics were created: `docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list`

### Database Issues
1. Ensure the SQL Server container is running: `docker ps | grep sqlserver`
2. Check the logs for database errors: `docker logs sqlserver` 