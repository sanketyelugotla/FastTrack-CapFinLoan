# CapFinLoan System Design Document

This document outlines the high-level architecture, use cases, database models, and low-level design patterns implemented in the CapFinLoan microservices ecosystem.

---

## 1. Use Case Diagram

This diagram maps out the primary actors (Applicant and Admin) and their interactions with the system capabilities.

```mermaid
flowchart LR
    Applicant((Applicant))
    Admin((Admin))
    
    subgraph CapFinLoan System
        UC1([Sign Up / Log In - OTP])
        UC2([Create/Update Draft Application])
        UC3([Submit Application])
        UC4([Upload Documents])
        UC5([Check Status])
        
        UC6([View Application Queue])
        UC7([Verify/Reject Documents])
        UC8([Review Application])
        UC9([Approve with Sanction/Interest])
        UC10([Reject Application])
        UC11([View Dashboard Metrics])
    end
    
    Applicant --> UC1
    Applicant --> UC2
    Applicant --> UC3
    Applicant --> UC4
    Applicant --> UC5
    
    Admin --> UC1
    Admin --> UC6
    Admin --> UC7
    Admin --> UC8
    Admin --> UC9
    Admin --> UC10
    Admin --> UC11
```

---

## 2. Microservices Architecture Diagram

This flowchart visualizes the topological layout of the microservices, showing how external requests arrive through an API Gateway, and how services communicate via HTTP (synchronous) or RabbitMQ (asynchronous).

```mermaid
graph TD
    %% Actors
    Client[Client Apps - Web/Mobile] --> Gateway[API Gateway - Ocelot/YARP]
    
    %% Microservices
    Gateway -->|HTTP REST| AuthAPI[Auth Service]
    Gateway -->|HTTP REST| AppAPI[Application Service]
    Gateway -->|HTTP REST| AdminAPI[Admin Service]
    Gateway -->|HTTP REST| DocAPI[Document Service]
    
    %% Internal Sync HTTP
    AdminAPI -.->|Internal HTTP| DocAPI
    AdminAPI -.->|Internal HTTP| AuthAPI
    AppAPI -.->|Internal HTTP| AuthAPI
    
    %% Databases
    AuthAPI -->|EF Core MySQL/SQL| AuthDB[(Auth DB)]
    AppAPI -->|EF Core MySQL/SQL| AppDB[(Application DB)]
    AdminAPI -->|EF Core MySQL/SQL| AdminDB[(Admin DB)]
    DocAPI -->|EF Core MySQL/SQL| DocDB[(Document DB)]
    
    %% Message Broker - RabbitMQ
    AuthAPI -->|Publish Events| RabbitMQ{RabbitMQ Event Bus}
    AppAPI -->|Publish Events| RabbitMQ
    AdminAPI -->|Publish Events| RabbitMQ
    DocAPI -->|Publish Events| RabbitMQ
    
    %% Notification Service purely consumes
    RabbitMQ -->|Consume Events| NotificationService[Notification Service]
    NotificationService -->|SMTP| EmailProvider[Email Provider]
    
    %% Saga Flow Examples
    RabbitMQ -->|Consume App Submitted| AdminAPI
    RabbitMQ -->|Consume Status Sync| AppAPI
```

---

## 3. Database Entity Relationship (ER) Diagram

Since this is a microservices architecture, tables are logically separated by domain. Note the Data Duplication (Data Syncing) strategy between `ApplicationService` and `AdminService`.

```mermaid
erDiagram
    %% Auth Service DB
    USER {
        Guid Id PK
        string Email
        string PasswordHash
        string Role
        bool IsActive
        DateTime CreatedAt
    }
    OTP {
        Guid Id PK
        string Email
        string OtpCode
        DateTime ExpiryTime
        bool IsUsed
    }
    
    %% Application Service DB
    APP_LOAN_APPLICATION {
        Guid Id PK
        Guid ApplicantUserId FK
        string ApplicationNumber
        string Status "Draft, Submitted"
        decimal RequestedAmount
        int RequestedTenure
        decimal MonthlyIncome
    }
    APP_STATUS_HISTORY {
        Guid Id PK
        Guid ApplicationId FK
        string Status
        DateTime ChangedAt
    }
    
    %% Admin Service DB (Sync Replica)
    ADMIN_LOAN_APPLICATION {
        Guid Id PK
        Guid ApplicantUserId FK
        string ApplicationNumber
        string Status "Submitted, UnderReview, Approved, Rejected, DocsPending"
    }
    ADMIN_DECISION {
        Guid Id PK
        Guid ApplicationId FK
        decimal SanctionAmount
        decimal InterestRate
        string Remarks
        Guid DecisionByUserId FK
    }
    
    %% Document Service DB
    LOAN_DOCUMENT {
        Guid Id PK
        Guid ApplicationId
        Guid UserId
        string FileName
        string StoredFileName
        string ContentType
        string DocumentType
        long FileSizeBytes
        string Status "Pending, Verified, ReuploadRequired"
        Guid VerifiedByUserId
        string Remarks
    }

    USER ||--o{ APP_LOAN_APPLICATION : creates
    APP_LOAN_APPLICATION ||--o{ APP_STATUS_HISTORY : has
    ADMIN_LOAN_APPLICATION ||--o{ ADMIN_DECISION : receives
```

---

## 4. Low Level Design (LLD) Document

### 4.1 Architecture Pattern: Clean Architecture + CQRS Lite
Each microservice is strictly divided into concentric layers to ensure separation of concerns:
* **API Layer**: Controllers, Middlewares (Global Exception Handler), and DI bootstrapping (`Program.cs`). Contains zero business logic.
* **Application Layer**: Contains Service Interfaces, DTOs (Contracts), and core Business Logic Services (`*Service.cs`).
* **Domain Layer**: Contains aggregate roots (Entities) and custom exception classes. Contains zero references to external libraries or databases.
* **Infrastructure Layer**: Contains `RabbitMQ` message publishers, `HttpClient` proxies, and `SMTP` implementations.
* **Persistence Layer**: EF Core `DbContext`, Migrations, and standard `IRepository` implementations.

### 4.2 Distributed Data Management Strategy
* **Database-per-Service**: Each service owns its schema. `ApplicationService` handles the early lifecycle (Draft to Submit). `AdminService` handles the late lifecycle.
* **Saga Pattern (Choreography)**: 
  * Instead of a central orchestrator, services react to Domain Events.
  * *Sync Scenario*: When `ApplicationService` pushes status to `Submitted`, it emits `ApplicationSubmittedEvent`. `AdminService` natively consumes this to project a replica of the application.
  * *Compensating Transaction Scenario*: If an Admin pushes status to `Approved`, `AdminService` emits `ApplicationStatusChangedEvent`. If `ApplicationService` fails to sync this status update in its own DB, it emits a `StatusSyncFailedEvent`. `AdminService` listens to this failure and rolls back the `Admin_Loan_Application` state and records a rollback remark.

### 4.3 Security & Resiliency
* **Authentication**: Centralized JWT issuance via `AuthService`. Standard `[Authorize]` attributes validate tokens across all microservices using a shared symmetric key injected via AppSettings.
* **Global Exception Handling**: All API services use a uniformly registered `GlobalExceptionHandlerMiddleware` which traps common custom exceptions (`InvalidOperationException`, `KeyNotFoundException`) and maps them to HTTP 400 and 404 responses respectively. This eliminates redundant `try/catch` blocs inside API Controllers.
* **Inter-Service Communication**: Restricted via `Internal` namespacing on routes. Some endpoints require a local bypass API Key (e.g., `X-Internal-Api-Key` headers) to prevent exposed exposure.

### 4.4 Unit Testing & Quality Assurance
* **Pattern**: Arrange-Act-Assert explicitly enforced. Mocking performed using `Moq`. Validation using `FluentAssertions`.
* **Testing Scope**: Tests target the `Application Layer` to test rigorous business boundaries (e.g., verifying `DocumentService` rejects >5MB files). Repositories and `HttpClient`s are fully mocked to prevent network/IO dependence.
* **Teardown Maintenance**: Handlers utilizing `IDisposable` (like `HttpClient` used in tests) are rigorously disposed in NUnit `[TearDown]` lifecycle attributes.
