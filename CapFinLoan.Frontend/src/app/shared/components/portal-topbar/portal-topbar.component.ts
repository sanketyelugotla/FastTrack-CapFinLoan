import { computed, Component, inject, input, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-portal-topbar',
  templateUrl: './portal-topbar.component.html',
  styleUrl: './portal-topbar.component.css'
})
export class PortalTopbarComponent {
  role = input.required<'ADMIN' | 'APPLICANT'>();

  auth = inject(AuthService);
  private router = inject(Router);
  private currentUrl = signal(this.router.url);

  portalLabel = computed(() => this.role() === 'ADMIN' ? 'Admin Portal' : 'Applicant Portal');

  sectionTitle = computed(() => {
    const url = this.currentUrl();

    if (this.role() === 'ADMIN') {
      if (url.includes('/reports')) return 'Reports Dashboard';
      if (url.includes('/users')) return 'User Directory';
      if (url.includes('/applications/')) return 'Application Review';
      if (url.includes('/queue')) return 'Applications Queue';
      return 'Admin Dashboard';
    }

    if (url.includes('/apply')) return 'Apply For A Loan';
    if (url.includes('/applications/') && url.includes('/documents')) return 'Documents';
    if (url.includes('/applications/') && url.includes('/status')) return 'Track Status';
    if (url.includes('/applications/')) return 'Application Details';
    if (url.includes('/applications')) return 'My Applications';
    if (url.includes('/profile')) return 'Profile';
    return 'Applicant Dashboard';
  });

  constructor() {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(event => this.currentUrl.set(event.urlAfterRedirects));
  }

  userInitial() {
    return this.auth.currentUser()?.name?.charAt(0)?.toUpperCase() ?? 'U';
  }
}