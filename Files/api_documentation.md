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
**Request Body:** *None* (Query Parameter: `?email=johndoe@example.com`)
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

### `GET /api/internal/users`
**Description:** Extracted user list for internal systems/Admins.
**Request Body:** *None*
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
**Request Body:** *None*
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
**Request Body:** *None* 
**Response Body:**
```json
{
  "id": "b3fc-...-...",
  "applicationNumber": "APP-10023",
  "status": "Submitted"
}
```

---

## 3. Admin Service API

### `GET /api/admin/applications`
**Description:** Fetches queue of all applications. Supports `?status=Submitted` filter.
**Request Body:** *None*
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
**Request Body:** *None*
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
**Request Body:** *None*
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

### `GET /api/admin/documents/{id}/download`  *(and `api/documents/{id}/download`)*
**Description:** Streams raw file bytes to client.
**Request Body:** *None*
**Response Body:** `[application/pdf or image/jpeg byte stream]`
