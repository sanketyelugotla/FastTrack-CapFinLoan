import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-signup',
  imports: [RouterLink, FormsModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  name = '';
  email = '';
  phone = '';
  password = '';
  isAdmin = false;
  loading = signal(false);
  error = signal('');

  onSubmit() {
    if (!this.name || !this.email || !this.phone || !this.password) return;
    this.loading.set(true);
    this.error.set('');

    const request = { name: this.name, email: this.email, phone: this.phone, password: this.password };
    const call = this.isAdmin
      ? this.authService.signupAdmin(request)
      : this.authService.signup(request);

    call.subscribe({
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
        this.error.set(err.error?.message || 'Signup failed. Please try again.');
      }
    });
  }
}
