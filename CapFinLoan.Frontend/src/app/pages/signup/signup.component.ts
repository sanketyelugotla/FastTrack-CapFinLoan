import { Component, inject, signal, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

declare var google: any;

type SignupStep = 'email' | 'otp' | 'details';

@Component({
  selector: 'app-signup',
  imports: [RouterLink, FormsModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent implements OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);

  step = signal<SignupStep>('email');
  loading = signal(false);
  error = signal('');
  info = signal('');

  // Step 1
  email = '';
  isAdmin = false;

  // Step 2
  otpCode = '';
  otpSecondsLeft = signal(0);
  private otpTimer: ReturnType<typeof setInterval> | null = null;

  // Step 3
  name = '';
  phone = '';
  password = '';
  confirmPassword = '';

  ngAfterViewInit() {
    this.initGoogleLogin();
  }

  private initGoogleLogin() {
    if (typeof google === 'undefined') {
      setTimeout(() => this.initGoogleLogin(), 100);
      return;
    }

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: this.handleGoogleCredentialResponse.bind(this)
    });

    google.accounts.id.renderButton(
      document.getElementById("google-btn-container"),
      { theme: "outline", size: "large", width: 350 }
    );
  }

  handleGoogleCredentialResponse(response: any) {
    if (response && response.credential) {
      this.loading.set(true);
      this.authService.loginWithGoogle(response.credential).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.role === 'ADMIN') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/applicant/dashboard']);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.error?.message || 'Google Signup failed.');
        }
      });
    }
  }

  // ─── Step 1: Send OTP ───────────────────────────────────────────────────────
  sendOtp() {
    if (!this.email) return;
    this.loading.set(true);
    this.error.set('');

    this.authService.sendSignupOtp(this.email).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.step.set('otp');
        this.info.set(`OTP sent to ${this.email}. Expires in ${res.expiryMinutes} minutes.`);
        this.startOtpTimer(res.expiryMinutes * 60);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to send OTP. Please try again.');
      }
    });
  }

  private startOtpTimer(seconds: number) {
    this.otpSecondsLeft.set(seconds);
    this.clearTimer();
    this.otpTimer = setInterval(() => {
      const left = this.otpSecondsLeft() - 1;
      this.otpSecondsLeft.set(left);
      if (left <= 0) {
        this.clearTimer();
        this.error.set('OTP expired. Please go back and request a new one.');
      }
    }, 1000);
  }

  get otpTimeLabel(): string {
    const s = this.otpSecondsLeft();
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${m}:${sec.toString().padStart(2, '0')}`;
  }

  // ─── Step 2: Validate OTP locally, then proceed ─────────────────────────────
  proceedToDetails() {
    if (!this.otpCode || this.otpCode.length !== 6 || !/^\d{6}$/.test(this.otpCode)) {
      this.error.set('Please enter a valid 6-digit OTP.');
      return;
    }
    if (this.otpSecondsLeft() <= 0) {
      this.error.set('OTP has expired. Go back and request a new one.');
      return;
    }
    this.error.set('');
    this.step.set('details');
    this.info.set('');
  }

  // ─── Step 3: Complete signup ─────────────────────────────────────────────────
  completeSignup() {
    if (!this.name || !this.phone || !this.password) return;
    if (this.password !== this.confirmPassword) {
      this.error.set('Passwords do not match.');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    const request = { email: this.email, otpCode: this.otpCode, name: this.name, phone: this.phone, password: this.password };
    const call = this.isAdmin
      ? this.authService.verifyOtpAndSignupAdmin(request)
      : this.authService.verifyOtpAndSignup(request);

    call.subscribe({
      next: (res) => {
        this.loading.set(false);
        this.clearTimer();
        if (res.role === 'ADMIN') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/applicant/dashboard']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Signup failed. Please try again.');
      }
    });
  }

  goBack() {
    this.error.set('');
    this.info.set('');
    if (this.step() === 'otp') {
      this.clearTimer();
      this.otpCode = '';
      this.step.set('email');
    } else if (this.step() === 'details') {
      this.step.set('otp');
    }
  }

  private clearTimer() {
    if (this.otpTimer) {
      clearInterval(this.otpTimer);
      this.otpTimer = null;
    }
  }

  ngOnDestroy() {
    this.clearTimer();
  }
}
