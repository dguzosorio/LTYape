# LTYape Microservices

Microservices-based application for processing financial transactions with fraud validation capabilities.

## Architecture

This solution consists of two main microservices:

1. **Transaction Service**: Handles transaction creation and management
2. **Anti-Fraud Service**: Validates transactions against fraud rules

The services communicate through Kafka messaging for asynchronous processing. Each service has its own database for storing domain-specific data.

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Clients / Frontend                            │
└───────────────────────────────────┬─────────────────────────────────────┘
                                    │
                                    ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                                                                           │
│  ┌────────────────────────────┐              ┌────────────────────────┐   │
│  │                            │              │                        │   │
│  │     TransactionService     │◄────────────►│    AntiFraudService    │   │
│  │                            │     Kafka    │                        │   │
│  └─────────────┬──────────────┘              └────────────┬───────────┘   │
│                │                                          │               │
│                │                                          │               │
│                │                                          │               │
│                ▼                                          ▼               │
│   ┌─────────────────────────┐                ┌─────────────────────────┐  │
│   │                         │                │                         │  │
│   │       SQL Server        │◄──────────────►│       SQL Server        │  │
│   │  (TransactionDB)        │                │    (AntiFraudDB)        │  │
│   │                         │                │                         │  │
│   └─────────────────────────┘                └─────────────────────────┘  │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

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
- **URL**: `POST http://localhost:5001/api/transactions`
- **Description**: Creates a new financial transaction
- **Request Body**: JSON with transaction details
- **Response**: Transaction details with a 201 Created status

#### Get Transaction
- **URL**: `GET http://localhost:5001/api/transactions`
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



## Troubleshooting

### Kafka Connection Issues
If you encounter issues connecting to Kafka, check:
1. Ensure the Kafka container is running: `docker ps | grep kafka`
2. Verify the Kafka topics were created: `docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list`

### Database Issues
1. Ensure the SQL Server container is running: `docker ps | grep sqlserver`
2. Check the logs for database errors: `docker logs sqlserver`

## Project Structure

The solution follows a clean architecture approach:

- **API Layer**: Controllers and configuration
- **Application Layer**: Application services and DTOs
- **Domain Layer**: Entities, repositories interfaces, and domain services
- **Infrastructure Layer**: Implementations of repositories, external services, and database contexts 