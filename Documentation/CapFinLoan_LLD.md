# CapFinLoan Financial Loan Management System
## Low-Level Design (LLD) Document
**Backend: ASP.NET Core 8 Microservices | Frontend: Angular 17**
**Version 2.0 | March 2026**

---

### 1. System Overview
CapFinLoan is a full-stack financial loan management platform composed of five ASP.NET Core 8 microservices behind an Ocelot API Gateway, with an Angular SPA as the client. Each service owns its own SQL Server database to ensure true microservice decoupling. Asynchronous inter-service communication is handled via RabbitMQ (MassTransit). Authentication uses JWT Bearer tokens validated independently across services.

#### 1.1 Architecture Summary

| Component | Technology | Responsibility |
| :--- | :--- | :--- |
| **Angular SPA** | Angular 17 / TypeScript | Applicant and Admin UI; standalone components. |
| **API Gateway** | Ocelot + SwaggerForOcelot | Single entry-point: routing, CORS, aggregated Swagger docs. |
| **Auth Service** | ASP.NET Core 8 | Auth, JWT, OTP, Google OAuth 2.0, user roles. |
| **Application Service** | ASP.NET Core 8 | Loan forms, EMI calculation, application status lifecycle. |
| **Document Service** | ASP.NET Core 8 | Secure document uploads, verification workflows. |
| **Admin Service** | ASP.NET Core 8 | Dashboards, audit logs, PDF report generation. |
| **Notification Service** | ASP.NET Core 8 | Dispatches email notifications to applicants. |
| **RabbitMQ** | rabbitmq:3-management | Async event bus; MassTransit. |
| **SQL Server** | sql-server | Isolated databases for each microservice. |

#### 1.2 Cross-Cutting Concerns
- **Authentication:** JWT Bearer tokens issued by the Auth Service.
- **Error Handling:** Global Exception Middleware mapping domain exceptions to standardised HTTP responses.
- **Repository Pattern:** Strict Clean Architecture (API, Application, Domain, Infrastructure, Persistence).

---

### 2. Shared Kernel
A class library referenced by all microservices to ensure consistency.

#### 2.1 Common DTOs
- `ApiResponse<T>`: Generic wrapper with Success (bool), Data (T), Message (string), Errors (List<string>).

#### 2.2 Domain Events (RabbitMQ)
- `UserRegisteredEvent`: Auth → Admin, Notification
- `ApplicationSubmittedEvent`: Application → Admin, Notification
- `DocumentVerificationUpdatedEvent`: Document → Application, Notification
- `ApplicationStatusChangedEvent`: Application → Notification, Admin

---

### 3. API Gateway (Ocelot)
Single entry-point for the Angular SPA. Handles routing and CORS.
**Route Table (ocelot.json)**
- `/auth/{everything}` → Auth Service
- `/applications/{everything}` → Application Service
- `/documents/{everything}` → Document Service
- `/admin/{everything}` → Admin Service

---

### 4. Auth Service
**Database: CapFinLoan_AuthDb**

#### 4.1 Data Models
- **User:** UserId (PK), FullName, Email, PhoneNumber, IsEmailVerified, PasswordHash, Role (Applicant/Admin).
- **OtpRecord:** Manages email verification codes.

#### 4.2 API Endpoints
- `POST /auth/register` - Create account, trigger OTP.
- `POST /auth/verify-register-otp` - Confirm email.
- `POST /auth/login` - Validate credentials, return JWT.
- `GET /auth/google` - Google OAuth integration.

---

### 5. Application Service
**Database: CapFinLoan_ApplicationDb**

#### 5.1 Data Models
- **LoanApplication:** ApplicationId (PK), UserId, RequestedAmount, Tenure, EMI, Status (Draft, Submitted, UnderReview, Approved, Rejected).
- **PersonalDetails:** One-to-One with LoanApplication.
- **EmploymentDetails:** EmployerName, MonthlyIncome.

#### 5.2 API Endpoints
- `POST /applications/calculate-emi` - Calculates potential EMI.
- `POST /applications` - Create a Draft application.
- `PUT /applications/{id}` - Update application details.
- `POST /applications/{id}/submit` - Transitions to Submitted.
- `GET /applications/me` - Get all applications for the logged-in user.

---

### 6. Document Service
**Database: CapFinLoan_DocumentDb**

#### 6.1 Data Models
- **LoanDocument:** DocumentId (PK), ApplicationId, UserId, DocumentType (IdProof, AddressProof, IncomeProof), FileUrl, Status (Pending, Verified, ReuploadRequired), Remarks.

#### 6.2 API Endpoints
- `POST /documents/upload` - Upload file, returns FileUrl.
- `PUT /documents/{id}/replace` - Replaces rejected documents.
- `GET /documents/application/{appId}` - Get all documents for an application.
- `PUT /documents/{id}/verify` - [Admin] Marks document as verified or requires reupload.

**Cross-Service Call:** The Document Service securely queries the Application Service (`GET /api/applications/{id}/status`) using a `TokenForwardingHandler` to ensure documents are locked if an application is already Approved/Rejected.

---

### 7. Admin Service
**Database: CapFinLoan_AdminDb**
Orchestrates dashboard data and reports by querying downstream services.

#### 7.1 API Endpoints
- `GET /admin/dashboard` - Aggregates key metrics.
- `GET /admin/applications` - Fetches all applications.
- `PUT /admin/applications/{id}/approve` - Approves a loan.
- `PUT /admin/applications/{id}/reject` - Rejects a loan.
- `POST /admin/reports/pdf` - Streams PDF report of loan statistics.

---

### 8. Frontend (Angular)
- **State Management:** Angular Signals for reactive UI updates.
- **Routing Guards:** `AuthGuard` and `RoleGuard` prevent unauthorized access to Applicant and Admin dashboards.
- **HTTP Interceptors:** `TokenInterceptor` automatically attaches the JWT Bearer token to all outgoing API Gateway requests.
