# CapFinLoan
## Financial Loan Management System
### High-Level System Overview
**Version 2.0 | March 2026**
**Prepared for non-technical stakeholders**

---

### 1. What Is CapFinLoan?
CapFinLoan is a complete, end-to-end financial loan management platform built to handle every stage of the loan journey — from a customer calculating potential EMIs online, all the way through applying for a loan, uploading verification documents, and receiving an approval or rejection decision.

The platform serves two distinct groups of people:
- **Applicants (Customers)** — who can calculate loan EMIs, submit loan applications, upload required identity and income proofs, and track their application status from any device.
- **Administrators (Loan Officers)** — who manage the entire operation: reviewing applications, verifying documents, approving or rejecting loans, and generating reports for business decisions.

**In Plain Terms**
Think of CapFinLoan as the digital backbone of a lending institution. Everything that would normally require phone calls, paper forms, or visits to a branch office can be done through CapFinLoan's website — quickly, securely, and with a complete audit trail.

---

### 2. Who Uses CapFinLoan?
The system is designed around two user roles, each with its own interface and permissions.

| Role | Who They Are | What They Can Do |
| :--- | :--- | :--- |
| **Applicant** | Any registered member of the public | Calculate EMIs, apply for loans, upload documents, track application status, manage their profile. |
| **Administrator** | Lending institution staff with elevated access | Manage all users and applications, review and verify documents, approve/reject loans, generate business reports, view full audit history. |

---

### 3. What the System Does
CapFinLoan covers five core functions, each operating independently but connected behind the scenes via a microservices architecture.

#### 3.1 Identity & Account Management
This covers everything related to user accounts and access control.
- Applicants can register with their email and password, or log in instantly using their Google account.
- A one-time verification code (OTP) is sent by email when a new account is created, confirming ownership of the email address.
- Applicants can reset forgotten passwords via an emailed verification code.
- Administrators can view all users, monitor registrations, or remove accounts when needed.

#### 3.2 EMI Calculator & Loan Products
CapFinLoan supports an interactive loan calculator to help applicants understand their financial commitments.
- **EMI Calculation** — Based on the requested loan amount, interest rate, and tenure (months or years), the system instantly calculates the monthly EMI, total interest payable, and the total payment amount.
- **Loan Types** — The system can handle various types of loans (e.g., Personal Loans, Home Loans, Auto Loans), each with specific document requirements and interest rate configurations.

#### 3.3 Loan Application Process
Applying for a loan is a streamlined, wizard-like process:
- Applicants provide personal details, contact information, and employment details (including employer name and monthly income).
- They specify the loan amount they are requesting.
- The application starts in a 'Draft' state, allowing the applicant to save their progress. Once all details are finalized, it moves to 'Submitted'.

#### 3.4 Document Management & Verification
A critical part of loan approval is verifying the applicant's identity and financial standing.
- Applicants must upload required documents (ID Proof, Address Proof, Income Proof, Bank Statements) directly through the platform. Documents are stored securely.
- On the admin side:
  - Administrators see all submitted documents in a central review dashboard.
  - Documents can be marked as 'Verified' or 'ReuploadRequired'. If re-upload is required, the admin provides remarks explaining why (e.g., "Image is blurry").
  - The applicant receives an automatic email notification if they need to fix their documents.
  - Once all documents are verified, the application becomes ready for a final decision.

#### 3.5 Administration & Reporting
Administrators have a dedicated control panel that provides:
- A live dashboard showing key metrics: total applicants, active applications, approval rates, and pending document reviews.
- The ability to make final decisions on applications (Approve or Reject).
- The ability to generate detailed reports covering application summaries, approval/rejection rates, and user activity — downloadable as PDF files.
- A full audit log: every significant action in the system is automatically recorded for accountability.

---

### 4. How It Works — Key User Journeys
The following section walks through the most common activities in plain language.

#### 4.1 Signing Up & Getting Started
1. Applicant visits the website and clicks Register.
2. They fill in their name, email, phone number, and a password (or choose to sign in with Google).
3. A 6-digit verification code is emailed to them. They enter it to confirm their email address.
4. Their account is now active. They are directed to their personal dashboard.

#### 4.2 Applying for a Loan
1. Applicant uses the EMI Calculator to estimate their monthly payments.
2. They start a new loan application, filling in personal and employment details.
3. They submit the application and are prompted to upload the required supporting documents.
4. The application appears in their dashboard with a "Submitted" status, and they await administrative review.

#### 4.3 Document Verification & Final Decision
1. An admin reviews the submitted application and opens the Document Review panel.
2. The admin checks each document. If a document is invalid, they mark it for re-upload with a comment.
3. The applicant receives an email, logs in, and uploads the corrected document.
4. Once the admin marks all documents as 'Verified', the application is ready for a final decision.
5. The admin Approves or Rejects the loan. The applicant receives an email notification with the final decision, and the system locks the application from further edits.

---

### 5. Security & Data Protection
CapFinLoan has been built with data security as a foundational requirement, not an afterthought.

| Security Feature | What It Means in Practice |
| :--- | :--- |
| **Password Protection** | Passwords are never stored in readable form. They are converted into a one-way encrypted format that cannot be reversed. |
| **Secure Login Sessions** | When a user logs in, they receive a temporary digital pass (JWT token) that expires after a set period. |
| **Email Verification** | New accounts require email confirmation via a time-limited one-time code, preventing fake registrations. |
| **Role-Based Access** | Applicants can only see and act on their own data. Admins have broader access, but admin actions are logged. |
| **Secure File Storage** | Sensitive financial and identity documents are securely stored and associated uniquely with the applicant. |
| **OTP Lockout Protection** | If someone attempts to guess a one-time verification code more than 5 times, the system locks that attempt automatically. |
| **Google Sign-In** | Applicants using Google to log in never need a password in CapFinLoan. Their Google identity is verified securely. |

---

### 6. System Components at a Glance
CapFinLoan is built as independent but connected microservices. Each module handles a specific area of responsibility and communicates with the others automatically in the background (using RabbitMQ).

| Module | Responsibility |
| :--- | :--- |
| **Identity & Auth Service** | Handles user registration, login, password resets, email verification, and Google sign-in. Manages roles. |
| **Application Service** | Manages the loan application forms, EMI calculations, and the overall status of the loan (Draft to Approved/Rejected). |
| **Document Service** | Handles the secure upload, storage, verification status, and replacement of sensitive applicant documents. |
| **Admin & Reporting Service** | Provides administrators with oversight tools: dashboards, audit logs, and PDF report generation. |
| **Notification Service** | Listens for system events and dispatches automated emails to applicants (e.g., OTPs, status changes, document issues). |

**Background Communication**
The modules automatically notify each other when important events happen via an event bus. For example, when a document is marked as requiring re-upload, the Document Service publishes an event, and the Notification Service instantly sends an email to the applicant.

---

### 7. Supported Document Types
The platform enforces strict rules on the types of documents that can be uploaded to ensure security and compliance.
- **Accepted Formats:** PDF, JPG, PNG
- **Categories:** ID Proof, Address Proof, Income Proof (Salary Slips/ITR), Bank Statements.

---

### 8. Applicant Notifications
CapFinLoan keeps applicants informed at every step through automated email notifications.
- **Account registered:** Welcome email with verification code.
- **Password reset requested:** Email with reset verification code.
- **Application submitted:** Admin team sees it in their queue.
- **Document re-upload required:** Applicant receives an email detailing which document needs fixing and why.
- **Loan Approved/Rejected:** Applicant receives an email confirmation of the final decision.

---

### 9. Reporting Capabilities
Administrators can generate business reports, downloadable as a PDF:
- **Application Summary:** Overview of all loan applications across the platform, grouped by status.
- **Verification Metrics:** Breakdown of document processing efficiency.
- **User Activity:** Registration trends and active users.

All reports are generated seamlessly and support accountability and audit requirements.

---

### 10. Summary
CapFinLoan is a modern, fully self-contained financial loan management platform that brings the entire borrowing journey online — from EMI calculation to final loan approval — while giving administrators the tools and visibility they need to run the lending business securely and efficiently.

**Key highlights:**
- Complete self-service experience for loan applicants.
- Structured, trackable document verification workflows ensuring compliance.
- Microservices architecture providing high security, scalability, and performance.
- Role-based access and a full audit trail protecting both applicants and the institution.
- Automatic email notifications keeping applicants informed without manual effort.
- Built-in EMI calculators and detailed reporting tools.

---
*This document is a high-level overview intended for business stakeholders and non-technical audiences. It summarises the capabilities and design of CapFinLoan version 2.0 as of March 2026.*
