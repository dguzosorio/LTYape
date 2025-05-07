# Sistema Anti-Fraude para Transacciones Financieras

Este proyecto implementa un sistema anti-fraude para transacciones financieras utilizando una arquitectura hexagonal (Puertos y Adaptadores) con dos microservicios principales:

1. **TransactionService**: Servicio para crear y consultar transacciones financieras.
2. **AntiFraudService**: Servicio para validar transacciones según reglas anti-fraude.

## Arquitectura del Sistema

El sistema sigue una arquitectura hexagonal (puertos y adaptadores) que separa las capas de:

- **Dominio**: Contiene la lógica de negocio y las entidades.
- **Aplicación**: Contiene los casos de uso y la orquestación de la lógica de negocio.
- **Infraestructura**: Implementa los adaptadores para comunicarse con servicios externos.
- **API**: Expone los endpoints HTTP para interactuar con el sistema.

### Diagrama de Arquitectura

```
                ┌───────────────────┐      ┌───────────────────┐
                │  TransactionService│      │  AntiFraudService │
                └─────────┬─────────┘      └─────────┬─────────┘
                          │                          │
                          ▼                          ▼
┌─────────────────────────────────────┐   ┌─────────────────────────────────┐
│           API Layer                 │   │           API Layer             │
└─────────────┬───────────────────────┘   └───────────────┬─────────────────┘
              │                                           │
              ▼                                           ▼
┌─────────────────────────────────────┐   ┌─────────────────────────────────┐
│        Application Layer            │   │        Application Layer        │
└─────────────┬───────────────────────┘   └───────────────┬─────────────────┘
              │                                           │
              ▼                                           ▼
┌─────────────────────────────────────┐   ┌─────────────────────────────────┐
│         Domain Layer                │   │         Domain Layer            │
└─────────────┬───────────────────────┘   └───────────────┬─────────────────┘
              │                                           │
              ▼                                           ▼
┌─────────────────────────────────────┐   ┌─────────────────────────────────┐
│      Infrastructure Layer           │   │      Infrastructure Layer       │
└─────────────┬───────────────────────┘   └───────────────┬─────────────────┘
              │                                           │
              ▼                                           ▼
      ┌───────────────┐                          ┌───────────────┐
      │  SQL Server   │◄────────────────────────►│    Kafka      │
      └───────────────┘                          └───────────────┘
```

### Flujo de Comunicación

1. **TransactionService** recibe una solicitud para crear una transacción.
2. La transacción se guarda en estado "pending" en la base de datos.
3. Un mensaje se envía a través de Kafka para que sea validado por **AntiFraudService**.
4. **AntiFraudService** recibe el mensaje, valida la transacción según las reglas anti-fraude.
5. **AntiFraudService** envía un mensaje de respuesta a través de Kafka.
6. **TransactionService** recibe la respuesta y actualiza el estado de la transacción a "approved" o "rejected".

## Reglas de Validación

Las transacciones se rechazan cuando:
- El valor de la transacción es mayor a 2000.
- El acumulado por día es mayor a 20000.

## Tecnologías Utilizadas

- **.NET 8**: Framework para el desarrollo de aplicaciones.
- **SQL Server**: Base de datos relacional.
- **Kafka**: Sistema de mensajería para la comunicación entre microservicios.
- **Docker/Docker Compose**: Para la contenerización y orquestación de los servicios.

## Cómo Ejecutar el Proyecto

### Requisitos Previos

- Docker y Docker Compose instalados.
- .NET 8 SDK para desarrollo local.

### Pasos para Ejecutar

1. Clonar el repositorio:
   ```
   git clone <repo-url>
   cd LTYape
   ```

2. Iniciar los servicios con Docker Compose:
   ```
   docker-compose up -d
   ```

3. Acceder a los endpoints:
   - TransactionService: http://localhost:5001
   - AntiFraudService: http://localhost:5002

## Endpoints Disponibles

### TransactionService

- **POST /api/transactions**: Crear una nueva transacción.
  ```json
  {
    "sourceAccountId": "Guid",
    "targetAccountId": "Guid",
    "tranferTypeId": 1,
    "value": 120
  }
  ```

- **GET /api/transactions/{transactionExternalId}**: Obtener una transacción por su ID externo.

### AntiFraudService

- Servicios internos para validación de fraude (no expuestos directamente).

## Desarrollo Local

Para desarrollar localmente fuera de Docker:

1. Configurar las variables de entorno apropiadas para apuntar a los servicios en contenedores.
2. Ejecutar los proyectos API con `dotnet run`.

## Estrategia de Testing

- **Unit Tests**: Para validar la lógica de negocio.
- **Integration Tests**: Para validar la interacción entre componentes.
- **E2E Tests**: Para validar el comportamiento completo del sistema. 