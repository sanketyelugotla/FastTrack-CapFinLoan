import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

declare var google: any;

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

  ngAfterViewInit() {
    this.initGoogleLogin();
  }

  private initGoogleLogin() {
    if (typeof google === 'undefined') {
      setTimeout(() => this.initGoogleLogin(), 100);
      return;
    }

    google.accounts.id.initialize({
      client_id: '142690176573-hkifvq2c70vd5qoi884vmpmjr6a1uag4.apps.googleusercontent.com',
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
}
