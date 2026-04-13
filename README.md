# CapFinLoan

CapFinLoan is a microservices-based loan processing platform with an Angular frontend and .NET backend services.

The platform includes:

- Authentication and user management
- Loan application creation and submission
- Document upload and verification
- Admin review and decision workflow
- Notification service (email and event-driven notifications)
- API Gateway for unified access

## Tech Stack

- Backend: .NET 10, ASP.NET Core Web API, EF Core
- Frontend: Angular 21
- Messaging: RabbitMQ + MassTransit
- Database: SQL Server
- Containerization: Docker + Docker Compose

## Prerequisites

Install these before running the project:

- Docker Desktop (latest stable)
- .NET SDK 10.0
- Node.js 20+ and npm 9+
- PowerShell 5.1+ (for startup scripts)

## Environment Setup

1. Go to project root.
2. Ensure `.env` exists in root and has email values for NotificationService.

Example keys:

- `EMAIL_USERNAME`
- `EMAIL_PASSWORD`
- `EMAIL_FROM_ADDRESS`
- `EMAIL_FROM_NAME`
- `EMAIL_ALERT_RECIPIENT`
- `COMPOSE_PARALLEL_LIMIT=6` (optional, improves Docker build speed)

## Run Option 1: Full Stack With Docker (Recommended)

From root folder:

```powershell
docker compose up -d --build --parallel
```

To view logs:

```powershell
docker compose logs -f
```

To stop:

```powershell
docker compose stop
```

To stop and remove containers:

```powershell
docker compose down
```

## Run Option 2: Backend Using Scripts + Frontend Local

This option starts backend services with the provided PowerShell scripts.

### 2.1 Start dependencies (SQL Server + RabbitMQ)

From root folder:

```powershell
docker compose up -d sqlserver rabbitmq
```

### 2.2 Start backend services

From `CapFinLoan.Backend`:

```powershell
./start-all.ps1
```

This builds once and starts:

- Auth Service
- Application Service
- Admin Service
- Document Service
- Notification Service
- API Gateway

To stop backend services started by script:

```powershell
./stop-all.ps1
```

### 2.3 Start frontend locally

From `CapFinLoan.Frontend`:

```powershell
npm install
npm start
```

## Service URLs

- Frontend: http://localhost:4200
- API Gateway: http://localhost:5020
- Auth Service: http://localhost:5021
- Application Service: http://localhost:5022
- Document Service: http://localhost:5023
- Admin Service: http://localhost:5024
- Notification Service: http://localhost:5025
- RabbitMQ Management: http://localhost:15673
- SQL Server: localhost,1433

## Solution Structure (Simple)

```text
CapFinLoan/
|-- CapFinLoan.Backend/
|   |-- AuthService/
|   |-- ApplicationService/
|   |-- DocumentService/
|   |-- AdminService/
|   |-- NotificationService/
|   |-- ApiGateway/
|   |-- Shared/
|   |-- start-all.ps1
|   `-- stop-all.ps1
|-- CapFinLoan.Frontend/
|-- docker-compose.yml
|-- .env
`-- Docs/
```

## Backend Layering Pattern (Per Service)

Each microservice typically follows:

- API: Controllers, middleware, startup config
- Application: Use cases, service interfaces, contracts (requests/responses)
- Domain: Entities, constants, core business rules
- Infrastructure: Messaging, external integrations
- Persistence: DbContext, repositories, migrations
- UnitTests: Service/business test coverage

## Useful Commands

Build all backend projects:

```powershell
cd CapFinLoan.Backend
dotnet build CapFinLoan.Backend.slnx
```

Run backend tests:

```powershell
cd CapFinLoan.Backend
dotnet test CapFinLoan.Backend.slnx
```

## Notes

- API Gateway is the preferred single entry point for frontend calls.
- Services communicate asynchronously via RabbitMQ events for Saga-style workflows.
- Notification failures are non-blocking for business flows (best effort with failure events).
- For local development speed, keep Docker BuildKit and compose parallel builds enabled.
