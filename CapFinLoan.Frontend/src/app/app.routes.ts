import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // ── Public Routes (with navbar + footer layout) ──
  {
    path: '',
    loadComponent: () => import('./layouts/public-layout/public-layout.component').then(m => m.PublicLayoutComponent),
    children: [
      { path: '', loadComponent: () => import('./pages/landing/landing.component').then(m => m.LandingComponent) }
    ]
  },

  // ── Auth Pages (no layout wrapper) ──
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'signup', loadComponent: () => import('./pages/signup/signup.component').then(m => m.SignupComponent) },

  // ── Applicant Routes ──
  {
    path: 'applicant',
    loadComponent: () => import('./layouts/applicant-layout/applicant-layout.component').then(m => m.ApplicantLayoutComponent),
    canActivate: [authGuard, roleGuard('APPLICANT')],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/applicant/dashboard/dashboard.component').then(m => m.ApplicantDashboardComponent) },
      { path: 'apply', loadComponent: () => import('./pages/applicant/apply-loan/apply-loan.component').then(m => m.ApplyLoanComponent) },
      { path: 'apply/:id', loadComponent: () => import('./pages/applicant/apply-loan/apply-loan.component').then(m => m.ApplyLoanComponent) },
      { path: 'applications', loadComponent: () => import('./pages/applicant/my-applications/my-applications.component').then(m => m.MyApplicationsComponent) },
      { path: 'track-status', loadComponent: () => import('./pages/applicant/track-status/track-status.component').then(m => m.TrackStatusComponent) },
      { path: 'applications/:id', loadComponent: () => import('./pages/applicant/application-detail/application-detail.component').then(m => m.ApplicationDetailComponent) },
      { path: 'applications/:id/status', loadComponent: () => import('./pages/applicant/track-status/track-status.component').then(m => m.TrackStatusComponent) },
      { path: 'applications/:id/documents', loadComponent: () => import('./pages/applicant/documents/documents.component').then(m => m.DocumentsComponent) },
      { path: 'profile', loadComponent: () => import('./pages/applicant/profile/profile.component').then(m => m.ProfileComponent) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // ── Admin Routes ──
  {
    path: 'admin',
    loadComponent: () => import('./layouts/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [authGuard, roleGuard('ADMIN')],
    children: [
      { path: 'dashboard', redirectTo: 'queue', pathMatch: 'full' },
      { path: 'queue', loadComponent: () => import('./pages/admin/queue/queue.component').then(m => m.AdminQueueComponent) },
      { path: 'applications/:id', loadComponent: () => import('./pages/admin/application-review/application-review.component').then(m => m.ApplicationReviewComponent) },
      { path: 'reports', loadComponent: () => import('./pages/admin/reports/reports.component').then(m => m.AdminReportsComponent) },
      { path: 'users', loadComponent: () => import('./pages/admin/users/users.component').then(m => m.AdminUsersComponent) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // ── Catch-all ──
  { path: '**', redirectTo: '' }
];
