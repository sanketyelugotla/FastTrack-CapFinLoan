import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { AuthService } from '../../../core/services/auth.service';
import { LoanApplicationResponse } from '../../../core/models/application.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-applicant-dashboard',
  imports: [RouterLink, StatusBadgeComponent, DatePipe, DecimalPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class ApplicantDashboardComponent implements OnInit {
  auth = inject(AuthService);
  private appService = inject(ApplicationService);

  applications = signal<LoanApplicationResponse[]>([]);
  loading = signal(true);

  private normalizeStatus(status: string): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  ngOnInit() {
    this.appService.getMyApplications().subscribe({
      next: (apps) => { this.applications.set(apps); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  firstName() {
    return this.auth.currentUser()?.name?.split(' ')[0] ?? 'Applicant';
  }

  countByStatus(status: string): number {
    const target = this.normalizeStatus(status);
    return this.applications().filter(a => this.normalizeStatus(a.status) === target).length;
  }

  pendingReviewCount() {
    return this.countByStatus('Submitted') + this.countByStatus('UnderReview') + this.countByStatus('DocsPending');
  }

  pendingRequirements() {
    const items: Array<{ title: string; detail: string; action: string; path: string | string[]; complete: boolean }> = [];
    const draft = this.applications().find(app => this.normalizeStatus(app.status) === 'draft');
    const docsPending = this.applications().find(app => this.normalizeStatus(app.status) === 'docspending');

    if (draft) {
      items.push({
        title: 'Complete draft application',
        detail: `${draft.applicationNumber} is still in draft and ready for completion.`,
        action: 'Resume',
        path: ['/applicant/applications', draft.id],
        complete: false
      });
    }

    if (docsPending) {
      items.push({
        title: 'Upload pending documents',
        detail: `${docsPending.applicationNumber} needs additional documents before review can continue.`,
        action: 'Upload',
        path: ['/applicant/applications', docsPending.id, 'documents'],
        complete: false
      });
    }

    if (!items.length) {
      items.push({
        title: 'Profile and documents look good',
        detail: 'No outstanding document or profile actions are blocking your current applications.',
        action: 'Review',
        path: '/applicant/profile',
        complete: true
      });
    }

    return items;
  }
}
