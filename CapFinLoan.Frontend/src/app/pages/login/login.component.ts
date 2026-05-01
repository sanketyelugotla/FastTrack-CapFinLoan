import { Component, inject, signal, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

declare var google: any;

type ForgotPasswordStep = 'email' | 'otp' | 'success';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  loading = signal(false);
  error = signal('');
  forgotPasswordOpen = signal(false);
  forgotPasswordStep = signal<ForgotPasswordStep>('email');
  forgotPasswordLoading = signal(false);
  forgotPasswordError = signal('');
  forgotPasswordInfo = signal('');
  forgotPasswordSecondsLeft = signal(0);

  forgotPasswordEmail = '';
  forgotPasswordOtp = '';
  forgotPasswordNewPassword = '';
  forgotPasswordConfirmPassword = '';
  private forgotPasswordTimer: ReturnType<typeof setInterval> | null = null;

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
          this.error.set(err.error?.message || 'Google Login failed.');
        }
      });
    }
  }

  onSubmit() {
    if (!this.email || !this.password) return;
    this.loading.set(true);
    this.error.set('');

    this.authService.login({ email: this.email, password: this.password }).subscribe({
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
        this.error.set(err.error?.message || 'Invalid email or password.');
      }
    });
  }

  openForgotPassword() {
    this.forgotPasswordOpen.set(true);
    this.forgotPasswordStep.set('email');
    this.forgotPasswordEmail = this.email;
    this.forgotPasswordOtp = '';
    this.forgotPasswordNewPassword = '';
    this.forgotPasswordConfirmPassword = '';
    this.forgotPasswordError.set('');
    this.forgotPasswordInfo.set('');
    this.clearForgotPasswordTimer();
  }

  closeForgotPassword() {
    this.forgotPasswordOpen.set(false);
    this.forgotPasswordStep.set('email');
    this.forgotPasswordError.set('');
    this.forgotPasswordInfo.set('');
    this.forgotPasswordLoading.set(false);
    this.clearForgotPasswordTimer();
  }

  sendForgotPasswordOtp() {
    if (!this.forgotPasswordEmail) {
      this.forgotPasswordError.set('Enter your email address to continue.');
      return;
    }

    this.forgotPasswordLoading.set(true);
    this.forgotPasswordError.set('');

    this.authService.sendForgotPasswordOtp(this.forgotPasswordEmail).subscribe({
      next: (response) => {
        this.forgotPasswordLoading.set(false);
        this.forgotPasswordStep.set('otp');
        this.forgotPasswordInfo.set(response.message);
        this.startForgotPasswordTimer(response.expiryMinutes * 60);
      },
      error: (err) => {
        this.forgotPasswordLoading.set(false);
        this.forgotPasswordError.set(err.error?.message || 'Unable to send OTP right now.');
      }
    });
  }

  resetPasswordWithOtp() {
    if (!/^\d{6}$/.test(this.forgotPasswordOtp)) {
      this.forgotPasswordError.set('Enter the 6-digit OTP sent to your email.');
      return;
    }

    if (!this.forgotPasswordNewPassword) {
      this.forgotPasswordError.set('Enter a new password.');
      return;
    }

    if (this.forgotPasswordNewPassword !== this.forgotPasswordConfirmPassword) {
      this.forgotPasswordError.set('Passwords do not match.');
      return;
    }

    this.forgotPasswordLoading.set(true);
    this.forgotPasswordError.set('');

    this.authService.resetPasswordWithOtp({
      email: this.forgotPasswordEmail,
      otpCode: this.forgotPasswordOtp,
      newPassword: this.forgotPasswordNewPassword
    }).subscribe({
      next: (response) => {
        this.forgotPasswordLoading.set(false);
        this.forgotPasswordStep.set('success');
        this.forgotPasswordInfo.set(response.message);
        this.password = '';
        this.clearForgotPasswordTimer();
      },
      error: (err) => {
        this.forgotPasswordLoading.set(false);
        this.forgotPasswordError.set(err.error?.message || 'Unable to reset password.');
      }
    });
  }

  resendForgotPasswordOtp() {
    this.sendForgotPasswordOtp();
  }

  private startForgotPasswordTimer(seconds: number) {
    this.forgotPasswordSecondsLeft.set(seconds);
    this.clearForgotPasswordTimer();
    this.forgotPasswordTimer = setInterval(() => {
      const nextValue = this.forgotPasswordSecondsLeft() - 1;
      this.forgotPasswordSecondsLeft.set(nextValue);
      if (nextValue <= 0) {
        this.clearForgotPasswordTimer();
      }
    }, 1000);
  }

  get forgotPasswordTimeLabel(): string {
    const totalSeconds = this.forgotPasswordSecondsLeft();
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  private clearForgotPasswordTimer() {
    if (this.forgotPasswordTimer) {
      clearInterval(this.forgotPasswordTimer);
      this.forgotPasswordTimer = null;
    }
  }

  ngOnDestroy() {
    this.clearForgotPasswordTimer();
  }
}
