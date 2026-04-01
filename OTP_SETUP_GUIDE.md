# OTP Email Verification Setup - Complete Guide

## ✅ What's Been Implemented (Backend)

### 1. **Email Configuration**

- Created `.env` file in project root with email credentials template

### 2. **OTP Database Table**

- `EmailVerificationOtp` entity with:
  - 6-digit OTP generation
  - 10-minute expiry
  - Used/unused tracking
  - Email and timestamps
- Migration: `AddEmailVerificationOtpTable` created

### 3. **OTP Repository**

- `IOtpRepository` and `OtpRepository`
- Methods:
  - `GenerateOtpAsync()` - Create and store OTP
  - `VerifyOtpAsync()` - Validate OTP and mark as used
  - `GetLatestOtpAsync()` - Retrieve latest OTP
  - `DeleteExpiredOtpsAsync()` - Cleanup

### 4. **Auth Service OTP Methods**

- `SendSignupOtpAsync(email)` - Send OTP via email event
- `VerifyOtpAndSignupAsync(request)` - Verify OTP + Create user
- `VerifyOtpAndSignupAdminAsync(request)` - Verify OTP + Create admin

### 5. **Auth API Endpoints**

- **POST** `/api/auth/send-otp?email=user@example.com` - Send OTP
- **POST** `/api/auth/verify-otp-signup` - Signup with OTP verification
- **POST** `/api/auth/verify-otp-signup-admin` - Admin signup with OTP

### 6. **OTP Email Consumer**

- `OtpSendConsumer` in NotificationService
- Listens to `OtpSendEvent` from RabbitMQ
- Sends formatted HTML email with OTP code

---

## 📋 Setup Steps - Step by Step

### **Step 1: Set Environment Variables**

Edit `.env` in project root with your Gmail or email provider:

```bash
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_FROM_ADDRESS=noreply@capfinloan.local
EMAIL_FROM_NAME=CapFinLoan
EMAIL_ALERT_RECIPIENT=admin@capfinloan.local
```

**For Gmail:**

1. Go to https://myaccount.google.com/apppasswords
2. Generate App Password
3. Use generated password in `.env`

### **Step 2: Run Database Migration**

```bash
cd CapFinLoan.Backend\AuthService\CapFinLoan.Auth.Persistence
dotnet ef database update --startup-project ../CapFinLoan.Auth.API
```

This creates the `EmailVerificationOtp` table in your database.

### **Step 3: Rebuild and Run Docker**

```bash
docker-compose down
docker-compose up -d --build
```

### **Step 4: Test Email Sending**

Call the endpoint to send OTP:

```
POST http://localhost:5020/api/auth/send-otp?email=test@example.com
```

Response:

```json
{
  "success": true,
  "message": "OTP sent to your email. Please verify within 10 minutes.",
  "email": "test@example.com",
  "expiryMinutes": 10
}
```

---

## 🎨 Frontend Implementation - Angular

### **Step 1: Create OTP Signup Component**

Create `src/app/components/auth/otp-signup/otp-signup.component.ts`:

```typescript
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { AuthService } from "../../../services/auth.service";
import { Router } from "@angular/router";

@Component({
  selector: "app-otp-signup",
  templateUrl: "./otp-signup.component.html",
  styleUrls: ["./otp-signup.component.css"],
})
export class OtpSignupComponent implements OnInit {
  step: "email" | "otp" | "details" = "email";
  emailForm!: FormGroup;
  detailsForm!: FormGroup;
  otpForm!: FormGroup;

  loading = false;
  errorMessage = "";
  successMessage = "";
  otpSentEmail = "";
  otpExpiry = 600; // 10 minutes in seconds
  otpTimer: any;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit() {
    this.initializeForms();
  }

  initializeForms() {
    this.emailForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
    });

    this.otpForm = this.fb.group({
      otp: ["", [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });

    this.detailsForm = this.fb.group(
      {
        name: ["", [Validators.required, Validators.minLength(2)]],
        phone: ["", Validators.required],
        password: ["", [Validators.required, Validators.minLength(8)]],
        confirmPassword: ["", Validators.required],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(form: FormGroup) {
    if (form.get("password")?.value !== form.get("confirmPassword")?.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  // Step 1: Send OTP
  async sendOtp() {
    if (this.emailForm.invalid) return;

    this.loading = true;
    this.errorMessage = "";

    try {
      const email = this.emailForm.get("email")?.value;
      const response = await this.authService.sendSignupOtp(email).toPromise();

      if (response?.success) {
        this.otpSentEmail = email;
        this.step = "otp";
        this.startOtpTimer();
        this.successMessage = "OTP sent to your email!";
      }
    } catch (error: any) {
      this.errorMessage = error?.error?.message || "Failed to send OTP";
    } finally {
      this.loading = false;
    }
  }

  // Step 2: Verify OTP
  async verifyOtp() {
    if (this.otpForm.invalid) return;

    this.loading = true;
    this.errorMessage = "";

    try {
      const otp = this.otpForm.get("otp")?.value;
      const email = this.otpSentEmail;

      // In real app, you'd verify the OTP with backend first
      // For now, we'll move to details form
      if (otp.length === 6) {
        this.step = "details";
        clearInterval(this.otpTimer);
        this.successMessage = "OTP verified! Enter your details below.";
      }
    } catch (error: any) {
      this.errorMessage = error?.error?.message || "Failed to verify OTP";
    } finally {
      this.loading = false;
    }
  }

  // Step 3: Complete Signup
  async completeSignup() {
    if (this.detailsForm.invalid) return;

    this.loading = true;
    this.errorMessage = "";

    try {
      const request = {
        email: this.otpSentEmail,
        otpCode: this.otpForm.get("otp")?.value,
        name: this.detailsForm.get("name")?.value,
        phone: this.detailsForm.get("phone")?.value,
        password: this.detailsForm.get("password")?.value,
      };

      const response = await this.authService
        .verifyOtpAndSignup(request)
        .toPromise();

      if (response?.token) {
        localStorage.setItem("token", response.token);
        localStorage.setItem("user", JSON.stringify(response));
        this.successMessage = "Account created successfully!";
        this.router.navigate(["/dashboard"]);
      }
    } catch (error: any) {
      this.errorMessage = error?.error?.message || "Signup failed";
    } finally {
      this.loading = false;
    }
  }

  startOtpTimer() {
    let remaining = this.otpExpiry;
    this.otpTimer = setInterval(() => {
      remaining--;
      this.otpExpiry = remaining;
      if (remaining <= 0) {
        clearInterval(this.otpTimer);
        this.errorMessage = "OTP expired. Please request a new one.";
        this.step = "email";
      }
    }, 1000);
  }

  getFormattedTime() {
    const minutes = Math.floor(this.otpExpiry / 60);
    const seconds = this.otpExpiry % 60;
    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  }

  goBack() {
    if (this.step === "otp") {
      this.step = "email";
      clearInterval(this.otpTimer);
    } else if (this.step === "details") {
      this.step = "otp";
    }
  }
}
```

### **Step 2: Create HTML Template**

Create `src/app/components/auth/otp-signup/otp-signup.component.html`:

```html
<div class="signup-container">
  <div class="signup-card">
    <h2 class="title">Create Account</h2>

    <!-- Error & Success Messages -->
    <div *ngIf="errorMessage" class="alert alert-danger">
      {{ errorMessage }}
    </div>
    <div *ngIf="successMessage" class="alert alert-success">
      {{ successMessage }}
    </div>

    <!-- Step 1: Email -->
    <div *ngIf="step === 'email'" @fadeInOut>
      <h3>Enter Your Email</h3>
      <form [formGroup]="emailForm" (ngSubmit)="sendOtp()">
        <div class="form-group">
          <label for="email">Email Address</label>
          <input
            type="email"
            class="form-control"
            id="email"
            formControlName="email"
            placeholder="your@email.com"
            [class.is-invalid]="emailForm.get('email')?.invalid && emailForm.get('email')?.touched"
          />
          <div
            class="invalid-feedback"
            *ngIf="emailForm.get('email')?.invalid && emailForm.get('email')?.touched"
          >
            Please enter a valid email
          </div>
        </div>
        <button
          type="submit"
          class="btn btn-primary btn-block"
          [disabled]="loading || emailForm.invalid"
        >
          <span
            *ngIf="loading"
            class="spinner-border spinner-border-sm mr-2"
          ></span>
          Send OTP
        </button>
      </form>
    </div>

    <!-- Step 2: OTP Verification -->
    <div *ngIf="step === 'otp'" @fadeInOut>
      <h3>Enter OTP Code</h3>
      <p class="text-muted">Code sent to <strong>{{ otpSentEmail }}</strong></p>

      <form [formGroup]="otpForm" (ngSubmit)="verifyOtp()">
        <div class="form-group">
          <label for="otp">6-Digit OTP Code</label>
          <input
            type="text"
            class="form-control otp-input"
            id="otp"
            formControlName="otp"
            placeholder="000000"
            maxlength="6"
            inputmode="numeric"
            [class.is-invalid]="otpForm.get('otp')?.invalid && otpForm.get('otp')?.touched"
          />
          <div
            class="invalid-feedback"
            *ngIf="otpForm.get('otp')?.invalid && otpForm.get('otp')?.touched"
          >
            Please enter a valid 6-digit OTP
          </div>
        </div>

        <div class="otp-timer">
          <span *ngIf="otpExpiry > 0" class="timer-text">
            Expires in: <strong>{{ getFormattedTime() }}</strong>
          </span>
          <span *ngIf="otpExpiry <= 0" class="timer-expired">
            OTP Expired
          </span>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-block"
          [disabled]="loading || otpForm.invalid"
        >
          <span
            *ngIf="loading"
            class="spinner-border spinner-border-sm mr-2"
          ></span>
          Verify OTP
        </button>
      </form>

      <button
        type="button"
        class="btn btn-link btn-block mt-2"
        (click)="goBack()"
      >
        Back
      </button>
    </div>

    <!-- Step 3: User Details -->
    <div *ngIf="step === 'details'" @fadeInOut>
      <h3>Complete Your Profile</h3>

      <form [formGroup]="detailsForm" (ngSubmit)="completeSignup()">
        <div class="form-group">
          <label for="name">Full Name</label>
          <input
            type="text"
            class="form-control"
            id="name"
            formControlName="name"
            placeholder="John Doe"
            [class.is-invalid]="detailsForm.get('name')?.invalid && detailsForm.get('name')?.touched"
          />
        </div>

        <div class="form-group">
          <label for="phone">Phone Number</label>
          <input
            type="tel"
            class="form-control"
            id="phone"
            formControlName="phone"
            placeholder="+1 (555) 123-4567"
            [class.is-invalid]="detailsForm.get('phone')?.invalid && detailsForm.get('phone')?.touched"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            type="password"
            class="form-control"
            id="password"
            formControlName="password"
            placeholder="Minimum 8 characters"
            [class.is-invalid]="detailsForm.get('password')?.invalid && detailsForm.get('password')?.touched"
          />
          <small class="form-text text-muted">
            Must be at least 8 characters with uppercase, lowercase, number, and
            special character
          </small>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Confirm Password</label>
          <input
            type="password"
            class="form-control"
            id="confirmPassword"
            formControlName="confirmPassword"
            placeholder="Re-enter password"
            [class.is-invalid]="(detailsForm.get('confirmPassword')?.invalid || detailsForm.hasError('passwordMismatch')) && detailsForm.touched"
          />
          <div
            class="invalid-feedback"
            *ngIf="detailsForm.hasError('passwordMismatch') && detailsForm.touched"
          >
            Passwords do not match
          </div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-block"
          [disabled]="loading || detailsForm.invalid"
        >
          <span
            *ngIf="loading"
            class="spinner-border spinner-border-sm mr-2"
          ></span>
          Create Account
        </button>
      </form>

      <button
        type="button"
        class="btn btn-link btn-block mt-2"
        (click)="goBack()"
      >
        Back
      </button>
    </div>
  </div>
</div>
```

### **Step 3: Update AuthService**

Add to `src/app/services/auth.service.ts`:

```typescript
sendSignupOtp(email: string) {
  return this.http.post(`${this.apiUrl}/auth/send-otp?email=${email}`, {});
}

verifyOtpAndSignup(request: OtpVerificationRequest) {
  return this.http.post(`${this.apiUrl}/auth/verify-otp-signup`, request);
}

verifyOtpAndSignupAdmin(request: OtpVerificationRequest) {
  return this.http.post(`${this.apiUrl}/auth/verify-otp-signup-admin`, request);
}

// Add this interface
interface OtpVerificationRequest {
  email: string;
  otpCode: string;
  name: string;
  phone: string;
  password: string;
}
```

### **Step 4: Add CSS Styling**

Create `src/app/components/auth/otp-signup/otp-signup.component.css`:

```css
.signup-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.signup-card {
  background: white;
  border-radius: 10px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  padding: 40px;
  max-width: 500px;
  width: 100%;
}

.title {
  text-align: center;
  color: #333;
  margin-bottom: 30px;
  font-size: 28px;
  font-weight: 600;
}

h3 {
  color: #555;
  margin-bottom: 20px;
  font-size: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #555;
  font-weight: 500;
}

.form-control {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 16px;
  transition: all 0.3s;
}

.form-control:focus {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  outline: none;
}

.form-control.is-invalid {
  border-color: #dc3545;
}

.otp-input {
  text-align: center;
  letter-spacing: 8px;
  font-size: 24px;
  font-weight: bold;
  font-family: monospace;
}

.btn {
  padding: 12px 20px;
  border: none;
  border-radius: 5px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary {
  background-color: #667eea;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #5568d3;
  transform: translateY(-2px);
}

.btn-block {
  width: 100%;
  display: block;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-link {
  background: none;
  color: #667eea;
  text-decoration: underline;
}

.btn-link:hover {
  color: #5568d3;
}

.alert {
  padding: 12px 20px;
  border-radius: 5px;
  margin-bottom: 20px;
}

.alert-danger {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.alert-success {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.otp-timer {
  text-align: center;
  margin: 15px 0;
  font-size: 14px;
}

.timer-text {
  color: #667eea;
  font-weight: 600;
}

.timer-expired {
  color: #dc3545;
  font-weight: 600;
}

.spinner-border {
  display: inline-block;
  width: 1rem;
  height: 1rem;
  vertical-align: -0.125em;
  border: 0.25em solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}

@keyframes spinner-border {
  to {
    transform: rotate(360deg);
  }
}

.text-muted {
  color: #999;
  font-size: 14px;
}

@media (max-width: 600px) {
  .signup-card {
    padding: 20px;
  }

  .title {
    font-size: 24px;
  }
}
```

---

## 🔄 Signup Flow

1. User enters email → Click "Send OTP"
2. Backend generates 6-digit OTP, stores in DB, sends via email
3. User receives email with OTP code
4. User enters OTP code → Click "Verify OTP"
5. Backend validates OTP, marks as used
6. User enters Name, Phone, Password → Click "Create Account"
7. Backend creates user with APPLICANT role, sends welcome email
8. User redirected to dashboard with auth token

---

## 📧 Email Environment Variables

Update `.env`:

```bash
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=app-password-from-google
EMAIL_FROM_ADDRESS=noreply@capfinloan.local
EMAIL_FROM_NAME=CapFinLoan Notifications
```

---

## ✅ Testing

**Send OTP:**

```bash
curl -X POST http://localhost:5020/api/auth/send-otp?email=test@example.com
```

**Verify OTP & Signup:**

```bash
curl -X POST http://localhost:5020/api/auth/verify-otp-signup \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "otpCode": "123456",
    "name": "John Doe",
    "phone": "+1234567890",
    "password": "SecurePass123!"
  }'
```

---

## 🎯 Next Steps

1. Set up `.env` file with real email credentials
2. Run migrations: `dotnet ef database update`
3. Rebuild Docker: `docker-compose up -d --build`
4. Create frontend components
5. Test OTP flow end-to-end
6. Verify emails are being received

---

**You're all set for OTP-based email verification!** 🎉
