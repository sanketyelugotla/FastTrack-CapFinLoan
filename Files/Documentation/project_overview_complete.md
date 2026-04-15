# CapFinLoan Overall Project Overview

This document consolidates all available project diagrams from the Files folder into a single reference.

## 1. Project Context and Scope

CapFinLoan is a microservices-based loan processing platform with role-based workflows for applicants and admins.

In-scope modules:

- Frontend UI
- API Gateway
- Auth Service
- Application Service
- Admin Service
- Document Service

Primary business outcomes:

- Secure user onboarding and authentication
- Loan application lifecycle management (draft to decision)
- Document collection and verification
- Admin operational review and decisioning

## 2. Core Components Summary

### 2.1 UI (Frontend)

- Provides applicant and admin journeys.
- Integrates all APIs through gateway routes.

### 2.2 API Gateway

- Single entry point for frontend requests.
- Handles route forwarding, policy integration, and unified access path.

### 2.3 Auth Service

- OTP and credential-based flows.
- JWT issuance and user account state handling.

### 2.4 Application Service

- Draft, update, submit, and status retrieval for loan applications.
- Owns submission validation and state transitions.

### 2.5 Admin Service

- Review queue and status decision actions.
- Enforces role-protected admin operations.

### 2.6 Document Service

- Upload/download and metadata persistence.
- File-type and size validations with ownership checks.

## 3. System Architecture

### 1.1 High-Level Architecture Diagram

![CapFinLoan Architecture](architecture_diagram.png)

## 4. Functional View

### 2.1 Use Case Diagram

![CapFinLoan Use Case Diagram](usecase_diagram.png)

## 5. Data View

### 3.1 Database ER Diagram

![CapFinLoan Database ER Diagram](database_er_diagram.png)

## 6. Sequence Diagrams

The following sequence diagrams capture the most important runtime interactions in the platform.

### 4.1 OTP Signup and Login Flow

Purpose: Shows OTP verification, user creation, and JWT-based login.

![OTP Signup and Login Sequence](sequence_diagrams/01_otp_signup_and_login.png)

### 4.2 Loan Application Draft and Submit Flow

Purpose: Shows draft persistence and final submission with event publishing.

![Application Submit Sequence](sequence_diagrams/02_application_submit_flow.png)

### 4.3 Document Upload Flow

Purpose: Shows file validation branch (valid/invalid) and metadata save path.

![Document Upload Sequence](sequence_diagrams/03_document_upload_flow.png)

### 4.4 Admin Review and Decision Flow

Purpose: Shows admin queue retrieval and decision propagation.

![Admin Review Sequence](sequence_diagrams/04_admin_review_and_decision.png)

### 4.5 Unauthorized Admin Route Access (403) Flow

Purpose: Shows security behavior when an applicant token tries to access admin endpoints.

![Unauthorized Admin Route Sequence](sequence_diagrams/05_admin_route_unauthorized_403.png)

## 7. Class Diagram

### 7.1 Core Domain Class Diagram

This class diagram shows primary domain entities and relationships across Auth, Application, Admin, and Document workflows.

![Core Domain Class Diagram](sequence_diagrams/06_core_domain_class_diagram.png)

## 8. Non-Functional Requirements (Summary)

- Security: JWT-based role checks, deny-by-default for restricted routes.
- Reliability: Global exception handling with standardized error payloads.
- Maintainability: Service-specific exception taxonomy and unit-test coverage.
- Observability: Structured errors and trace-friendly API behavior.

## 9. Delivery Timeline Mapping (Mar 21 - Apr 7)

- Sprint 1 (Mar 21 - Apr 3): Core implementation of auth, application, document, admin, and gateway integration.
- Sprint 2 (Apr 4 - Apr 7): Hardening, exception standardization, test updates, and documentation closure.

## 10. Diagram Index

- Architecture: [architecture_diagram.png](architecture_diagram.png)
- Use Case: [usecase_diagram.png](usecase_diagram.png)
- ER Diagram: [database_er_diagram.png](database_er_diagram.png)
- Sequence 1 (OTP): [sequence_diagrams/01_otp_signup_and_login.png](sequence_diagrams/01_otp_signup_and_login.png)
- Sequence 2 (Application): [sequence_diagrams/02_application_submit_flow.png](sequence_diagrams/02_application_submit_flow.png)
- Sequence 3 (Document): [sequence_diagrams/03_document_upload_flow.png](sequence_diagrams/03_document_upload_flow.png)
- Sequence 4 (Admin): [sequence_diagrams/04_admin_review_and_decision.png](sequence_diagrams/04_admin_review_and_decision.png)
- Sequence 5 (403 Security): [sequence_diagrams/05_admin_route_unauthorized_403.png](sequence_diagrams/05_admin_route_unauthorized_403.png)
- Class Diagram: [sequence_diagrams/06_core_domain_class_diagram.png](sequence_diagrams/06_core_domain_class_diagram.png)
- Class Diagram (Vector): [sequence_diagrams/06_core_domain_class_diagram.svg](sequence_diagrams/06_core_domain_class_diagram.svg)
