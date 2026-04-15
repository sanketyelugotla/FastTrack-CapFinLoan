# CapFinLoan Project Planning Artifacts

Date Range: March 21 to April 7
Planning Model: Agile (2-week sprint cadence recommended)
Tracking Tools: Azure Boards / Jira / Trello

## 1. Work Breakdown Structure (WBS)

### 1.1 UI (Frontend)

1. Requirements and UX flow
   - Finalize user journeys (signup/login, application lifecycle, document upload, admin review)
   - Create wireframes for applicant and admin screens
2. Frontend architecture and setup
   - Project structure, routing, auth guards, shared components
   - Environment config and API integration base URLs
3. Authentication module
   - Signup with OTP steps
   - Login, token storage, logout, role-based redirects
4. Applicant module
   - Create draft, edit draft, submit application
   - View status timeline and application details
5. Document module
   - Upload, validation messages, list and replace document
   - Download/view document
6. Admin module
   - Admin queue, application details
   - Status decision actions (approve/reject/docs pending)
   - Document verification actions
7. Testing and quality
   - Component/unit tests for critical forms and guards
   - E2E happy-path checks

### 1.2 API Gateway

1. Gateway setup and route configuration
   - Route definitions for Auth, Application, Admin, Document services
   - Service discovery/base URL mapping
2. Security and policies
   - Auth pass-through and header forwarding
   - CORS policy and role-protected route behavior
3. Reliability and observability
   - Error response pass-through standardization
   - Basic health endpoints and request logging
4. Validation and test
   - Route-level test cases and negative checks

### 1.3 Auth Service

1. Domain and data model
   - User and OTP entities, role definitions
2. Application and business logic
   - OTP generation/verification
   - Signup/login/google-login flows
   - User management methods (status update, notification info)
3. API layer
   - Auth controllers and request validation
   - Global exception handling
4. Integration
   - JWT generation
   - Event publishing for user registration and OTP send
5. Testing
   - Unit tests for OTP and login/signup scenarios

### 1.4 Application Service (Core)

1. Domain and data model
   - Loan application entity and status history
2. Application logic
   - Create draft, update draft, submit, fetch status
   - Validation rules for submission
3. API layer
   - Applicant endpoints and role checks
   - Global exception handling
4. Integration
   - Event publishing on submission and status changes
5. Testing
   - Unit tests for submit validations and status transitions

### 1.5 Admin Service (Reporting/Decision)

1. Domain and data model
   - Admin view of applications and decision records
2. Application logic
   - Queue retrieval, dashboard metrics
   - Status transition workflow and business rules
3. API layer
   - Admin endpoints and role-restricted actions
   - Global exception handling
4. Integration
   - Event publishing for application status updates
   - Internal API proxy calls where required
5. Testing
   - Unit tests for transition rules and approval logic

### 1.6 Document Service

1. Domain and data model
   - Document metadata, verification status model
2. Application logic
   - Upload, replace, link, download
   - File validation (type/size) and ownership checks
3. API layer
   - Applicant and internal/admin endpoints
   - Global exception handling
4. Integration and storage
   - Local storage/volume path handling
   - Verification event publishing
5. Testing
   - Unit tests for upload validations and verification outcomes

---

## 2. Agile Sprint Plan (March 21 to April 7)

### Sprint Cadence

- Recommended cadence: 2-week sprint
- Actual plan in this date window:
  - Sprint 1: March 21 to April 3 (14 days)
  - Sprint 2: April 4 to April 7 (4 days, hardening/release sprint)

### Board Setup (Azure Boards/Jira/Trello)

- Columns:
  - Backlog
  - Sprint Ready
  - In Progress
  - Code Review
  - QA/UAT
  - Done
- Labels/Tags:
  - `ui`, `gateway`, `auth`, `application`, `admin`, `document`, `test`, `bug`, `security`
- Estimation fields:
  - Story Points
  - Original Estimate (hours)
  - Remaining Work (hours)

---

## 3. Sprint-Wise Backlog

## Sprint 1 (March 21 - April 3)

Sprint Goal:
Deliver end-to-end loan journey baseline with authentication, draft/submit flow, document upload, admin decision workflow, and gateway route integration.

Definition of Done (Sprint 1):

- Code merged to main branch with peer review
- Unit tests for critical business logic pass
- API endpoints documented and callable via gateway
- No blocker severity defects in core flows
- Demo completed for applicant and admin base scenarios

| ID    | Story                                          | Service/Area          | Story Points | Est. Hours | Acceptance Criteria                                                        |
| ----- | ---------------------------------------------- | --------------------- | -----------: | ---------: | -------------------------------------------------------------------------- |
| S1-01 | Implement OTP signup and login APIs            | Auth Service          |            8 |         20 | Signup OTP, verify, login return valid token; negative validations handled |
| S1-02 | Implement applicant draft creation/edit/submit | Application Service   |            8 |         22 | Draft created, updated, submitted with validations and status history      |
| S1-03 | Implement document upload/list/download        | Document Service      |            8 |         20 | Supported files accepted, invalid files rejected, docs retrievable         |
| S1-04 | Implement admin queue and decision APIs        | Admin Service         |            8 |         22 | Queue loads, status changes enforce business rules                         |
| S1-05 | Configure gateway routes and policies          | API Gateway           |            5 |         12 | Frontend calls via gateway for all public endpoints                        |
| S1-06 | Build frontend auth and applicant pages        | UI                    |            8 |         24 | OTP signup/login and application screens functional                        |
| S1-07 | Build frontend document + admin pages          | UI                    |            8 |         24 | Upload flow, admin queue/details/decision screens functional               |
| S1-08 | Add core unit tests for service logic          | Cross-service Testing |            5 |         14 | Core positive/negative unit tests added and passing                        |

Sprint 1 Capacity Summary:

- Total Story Points: 58
- Total Estimated Hours: 158

## Sprint 2 (April 4 - April 7)

Sprint Goal:
Stabilize, close quality gaps, align exceptions/tests/docs, and prepare release-ready build.

Definition of Done (Sprint 2):

- All unit tests pass in CI/local full test run
- Error handling standardized across services
- Planning/docs/test matrix updated
- Release candidate validated via smoke tests

| ID    | Story                                     | Service/Area                    | Story Points | Est. Hours | Acceptance Criteria                                       |
| ----- | ----------------------------------------- | ------------------------------- | -----------: | ---------: | --------------------------------------------------------- |
| S2-01 | Refactor to custom exception taxonomy     | Auth/Application/Admin/Document |            5 |         12 | Service-specific exceptions mapped in global middleware   |
| S2-02 | Update unit tests for custom exceptions   | Auth/Application/Admin/Document |            5 |         10 | Legacy InvalidOperation assertions replaced; tests green  |
| S2-03 | Complete missing service-level test cases | Auth/Application/Admin          |            3 |          8 | Added tests for login success, draft create, admin queue  |
| S2-04 | Documentation hardening                   | Docs + API docs                 |            2 |          6 | API docs and planning artifacts updated and consistent    |
| S2-05 | End-to-end smoke + bug fixes              | UI + Gateway + Services         |            3 |         10 | Core applicant/admin scenarios validated without blockers |

Sprint 2 Capacity Summary:

- Total Story Points: 18
- Total Estimated Hours: 46

---

## 4. Delivery Notes for Faculty Review

- This plan covers UI, gateway, and 4 core services (Auth, Application, Admin, Document).
- Sprint 1 delivers functional scope; Sprint 2 focuses on quality and release hardening within the date window.
- A future Sprint 3 can be used for CI/CD pipeline hardening, performance tuning, and integration/security tests (including route-level 403 integration tests).
