# CapFinLoan API Documentation

## 1. Auth Service API

### `POST /api/auth/signup`

**Description:** Register a new Applicant user.
**Request Body:**

```json
{
  "email": "johndoe@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Response Body:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "User registered successfully."
}
```

### `POST /api/auth/send-otp`

**Description:** Sends an OTP to the given email via query.
**Request Body:** _None_ (Query Parameter: `?email=johndoe@example.com`)
**Response Body:**

```json
{
  "message": "OTP sent successfully."
}
```

### `POST /api/auth/verify-otp-signup`

**Description:** Validates OTP and activates the user.
**Request Body:**

```json
{
  "email": "johndoe@example.com",
  "otp": "123456"
}
```

**Response Body:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn..."
}
```

### `POST /api/auth/signup-admin`

**Description:** Register a new Admin user (legacy direct signup flow).
**Request Body:**

```json
{
  "name": "Admin User",
  "email": "admin@example.com",
  "phone": "+919999999999",
  "password": "Password123!"
}
```

**Response Body:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn...",
  "expiresAtUtc": "2026-04-13T14:15:00Z",
  "role": "ADMIN",
  "userId": "d8ab1cb9-4953-4e1f-8f43-ccfcd2c172d1",
  "name": "Admin User",
  "email": "admin@example.com"
}
```

### `POST /api/auth/verify-otp-signup-admin`

**Description:** Validates OTP and completes Admin signup.
**Request Body:**

```json
{
  "name": "Admin User",
  "email": "admin@example.com",
  "phone": "+919999999999",
  "password": "Password123!",
  "otpCode": "123456"
}
```

**Response Body:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn...",
  "expiresAtUtc": "2026-04-13T14:15:00Z",
  "role": "ADMIN",
  "userId": "d8ab1cb9-4953-4e1f-8f43-ccfcd2c172d1",
  "name": "Admin User",
  "email": "admin@example.com"
}
```

### `POST /api/auth/login`

**Description:** Logs in using Email and Password.
**Request Body:**

```json
{
  "email": "johndoe@example.com",
  "password": "Password123!"
}
```

**Response Body:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn..."
}
```

### `POST /api/auth/google-login`

**Description:** Logs in or auto-registers user using Google ID token.
**Request Body:**

```json
{
  "idToken": "google-id-token-value"
}
```

**Response Body:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn...",
  "expiresAtUtc": "2026-04-13T14:15:00Z",
  "role": "APPLICANT",
  "userId": "0b146e10-2fd8-49b1-b391-c0f6f1436d8b",
  "name": "John Doe",
  "email": "johndoe@gmail.com"
}
```

### `GET /api/internal/users`

**Description:** Extracted user list for internal systems/Admins.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "johndoe@example.com",
    "role": "Applicant",
    "isActive": true
  }
]
```

### `PUT /api/internal/users/{id}/status`

**Description:** Admin unblocks/blocks a user.
**Request Body:**

```json
{
  "isActive": false
}
```

**Response Body:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "johndoe@example.com",
  "isActive": false
}
```

---

## 2. Application Service API

### `GET /api/applications/my`

**Description:** Retrieves all applications belonging to the user.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "b3fc-...-...",
    "applicationNumber": "APP-10023",
    "status": "Draft",
    "requestedAmount": 50000,
    "requestedTenure": 24,
    "monthlyIncome": 8500
  }
]
```

### `POST /api/applications`

**Description:** Creates a new draft application.
**Request Body:**

```json
{
  "requestedAmount": 50000,
  "requestedTenure": 24,
  "monthlyIncome": 8500,
  "purpose": "Home Renovation",
  "employerName": "Capgemini"
}
```

**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Draft"
}
```

### `PUT /api/applications/{id}`

**Description:** Updates a draft application.
**Request Body:**

```json
{
  "requestedAmount": 60000,
  "requestedTenure": 36,
  "monthlyIncome": 8500
}
```

**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Draft"
}
```

### `POST /api/applications/{id}/submit`

**Description:** Transitions an application from Draft to Submitted.
**Request Body:** _None_
**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Submitted"
}
```

### `GET /api/applications/{id}`

**Description:** Retrieves one application by ID for applicant/admin access rules.
**Request Body:** _None_
**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Draft",
  "requestedAmount": 60000,
  "requestedTenure": 36,
  "monthlyIncome": 8500,
  "purpose": "Home Renovation"
}
```

### `GET /api/applications/{id}/status`

**Description:** Fetches only the latest status snapshot of an application.
**Request Body:** _None_
**Response Body:**

```json
{
  "applicationId": "b3fc-...-...",
  "status": "Submitted",
  "updatedAtUtc": "2026-04-13T12:45:00Z"
}
```

### `DELETE /api/applications/{id}`

**Description:** Deletes a draft application owned by the applicant.
**Request Body:** _None_
**Response Body:** _No content (HTTP 204)_

---

## 3. Admin Service API

### `GET /api/admin/applications`

**Description:** Fetches queue of all applications. Supports `?status=Submitted` filter.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "b3fc-...-...",
    "applicationNumber": "APP-10023",
    "status": "Submitted",
    "applicantId": "3fa85f64-..."
  }
]
```

### `GET /api/admin/applications/{id}`

**Description:** Admin fetches full app details.
**Request Body:** _None_
**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Submitted",
  "requestedAmount": 60000,
  "requestedTenure": 36,
  "monthlyIncome": 8500,
  "decisions": [
    {
      "status": "Submitted",
      "remarks": "Awaiting initial review"
    }
  ]
}
```

### `PUT /api/admin/applications/{id}/status`

**Description:** Admin assigns a new status (UnderReview, Approved, Rejected, DocsPending).
**Request Body:**

```json
{
  "newStatus": "Approved",
  "sanctionAmount": 55000,
  "interestRate": 8.5,
  "remarks": "Approved after manual document verification."
}
```

**Response Body:**

```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Approved"
}
```

### `GET /api/admin/applications/dashboard`

**Description:** Returns admin dashboard metrics.
**Request Body:** _None_
**Response Body:**

```json
{
  "totalApplications": 128,
  "submitted": 36,
  "underReview": 18,
  "approved": 52,
  "rejected": 22
}
```

### `GET /api/admin/documents/application/{applicationId}`

**Description:** Fetches all documents for a specific application from admin perspective.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "a2bc-...-...",
    "applicationId": "b3fc-...-...",
    "documentType": "SalarySlip",
    "status": "Pending",
    "fileName": "slip_april.pdf"
  }
]
```

### `GET /api/admin/documents`

**Description:** Fetches all documents across applications. Supports optional `?status=Pending`.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "a2bc-...-...",
    "applicationId": "b3fc-...-...",
    "documentType": "Aadhaar",
    "status": "Pending",
    "fileName": "aadhaar.pdf"
  }
]
```

---

## 4. Document Service API

### `POST /api/documents/upload`

**Description:** Uploads a physical file.
**Request Body:** `multipart/form-data`

- `applicationId`: `"b3fc-..."`
- `documentType`: `"SalarySlip"`
- `file`: `[File Bytes]`
  **Response Body:**

```json
{
  "id": "a2bc-...-...",
  "documentType": "SalarySlip",
  "status": "Pending",
  "fileName": "slip_april.pdf"
}
```

### `GET /api/documents/application/{id}`

**Description:** Metadata of all documents attached to an application.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "a2bc-...-...",
    "documentType": "SalarySlip",
    "status": "Pending",
    "fileName": "slip_april.pdf"
  }
]
```

### `PUT /api/admin/documents/{id}/verify`

**Description:** Admin verifies or rejects a document.
**Request Body:**

```json
{
  "isVerified": true,
  "remarks": "Looks clean."
}
```

**Response Body:**

```json
{
  "id": "a2bc-...-...",
  "status": "Verified",
  "remarks": "Looks clean."
}
```

### `GET /api/admin/documents/{id}/download` _(and `api/documents/{id}/download`)_

**Description:** Streams raw file bytes to client.
**Request Body:** _None_
**Response Body:** `[application/pdf or image/jpeg byte stream]`

### `GET /api/documents/my`

**Description:** Retrieves all documents uploaded by the current applicant user.
**Request Body:** _None_
**Response Body:**

```json
[
  {
    "id": "a2bc-...-...",
    "applicationId": "b3fc-...-...",
    "documentType": "SalarySlip",
    "status": "Pending",
    "fileName": "slip_april.pdf"
  }
]
```

### `GET /api/documents/{id}`

**Description:** Fetches metadata of a single document.
**Request Body:** _None_
**Response Body:**

```json
{
  "id": "a2bc-...-...",
  "applicationId": "b3fc-...-...",
  "documentType": "SalarySlip",
  "status": "Pending",
  "fileName": "slip_april.pdf",
  "uploadedAtUtc": "2026-04-13T09:10:00Z"
}
```

### `PUT /api/documents/{id}`

**Description:** Replaces an existing document file and optionally updates document type.
**Request Body:** `multipart/form-data`

- `file`: `[File Bytes]`
- `documentType`: `"BankStatement"` _(optional)_
  **Response Body:**

```json
{
  "id": "a2bc-...-...",
  "documentType": "BankStatement",
  "status": "Pending",
  "fileName": "statement_march.pdf"
}
```

### `POST /api/documents/{id}/link`

**Description:** Links an existing uploaded document to another application.
**Request Body:**

```json
{
  "applicationId": "b3fc-...-..."
}
```

**Response Body:**

```json
{
  "id": "a2bc-...-...",
  "applicationId": "b3fc-...-...",
  "message": "Document linked successfully."
}
```
