import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, OtpSendResponse, OtpVerificationRequest, SignupRequest } from '../models/auth.models';

interface StoredUser {
  userId: string;
  name: string;
  email: string;
  role: string;
  token: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);
  private apiUrl = `${environment.apiBaseUrl}/auth`;

  currentUser = signal<StoredUser | null>(null);
  isLoggedIn = computed(() => !!this.currentUser());
  isAdmin = computed(() => this.currentUser()?.role === 'ADMIN');
  isApplicant = computed(() => this.currentUser()?.role === 'APPLICANT');

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const stored = localStorage.getItem('capfinloan_user');
      if (stored) {
        try {
          const user: StoredUser = JSON.parse(stored);
          const expires = new Date(user.expiresAtUtc);
          if (expires > new Date()) {
            this.currentUser.set(user);
          } else {
            localStorage.removeItem('capfinloan_user');
          }
        } catch {
          localStorage.removeItem('capfinloan_user');
        }
      }
    }
  }

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(res => this.storeUser(res))
    );
  }

  signup(request: SignupRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/signup`, request).pipe(
      tap(res => this.storeUser(res))
    );
  }

  loginWithGoogle(idToken: string) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google-login`, { idToken }).pipe(
      tap(res => this.storeUser(res))
    );
  }

  signupAdmin(request: SignupRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/signup-admin`, request).pipe(
      tap(res => this.storeUser(res))
    );
  }

  sendSignupOtp(email: string) {
    return this.http.post<OtpSendResponse>(`${this.apiUrl}/send-otp?email=${encodeURIComponent(email)}`, {});
  }

  verifyOtpAndSignup(request: OtpVerificationRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/verify-otp-signup`, request).pipe(
      tap(res => this.storeUser(res))
    );
  }

  verifyOtpAndSignupAdmin(request: OtpVerificationRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/verify-otp-signup-admin`, request).pipe(
      tap(res => this.storeUser(res))
    );
  }

  logout() {
    this.currentUser.set(null);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('capfinloan_user');
    }
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  private storeUser(res: AuthResponse) {
    const user: StoredUser = {
      userId: res.userId,
      name: res.name,
      email: res.email,
      role: res.role,
      token: res.token,
      expiresAtUtc: res.expiresAtUtc
    };
    this.currentUser.set(user);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('capfinloan_user', JSON.stringify(user));
    }
  }
}
