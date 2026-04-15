# CapFinLoan API Gateway Routes

This file lists all externally accessible API routes.

## Auth Service

- `POST /api/auth/signup`
- `POST /api/auth/signup-admin`
- `POST /api/auth/send-otp`
- `POST /api/auth/verify-otp-signup`
- `POST /api/auth/verify-otp-signup-admin`
- `POST /api/auth/login`
- `POST /api/auth/google-login`

## Application Service

- `GET /api/applications/my`
- `GET /api/applications/{id:guid}`
- `POST /api/applications`
- `PUT /api/applications/{id:guid}`
- `POST /api/applications/{id:guid}/submit`
- `GET /api/applications/{id:guid}/status`
- `DELETE /api/applications/{id:guid}`

## Admin Service

### Admin Applications

- `GET /api/admin/applications`
- `GET /api/admin/applications/dashboard`
- `GET /api/admin/applications/{id:guid}`
- `PUT /api/admin/applications/{id:guid}/status`

### Admin Documents

- `GET /api/admin/documents/application/{applicationId:guid}`
- `GET /api/admin/documents`
- `GET /api/admin/documents/{id:guid}/download`
- `PUT /api/admin/documents/{id:guid}/verify`

## Document Service

- `POST /api/documents/upload`
- `GET /api/documents/application/{applicationId:guid}`
- `GET /api/documents/my`
- `GET /api/documents/{id:guid}`
- `PUT /api/documents/{id:guid}`
- `POST /api/documents/{id:guid}/link`
- `GET /api/documents/{id:guid}/download`
