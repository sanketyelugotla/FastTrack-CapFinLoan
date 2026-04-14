import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { LoanApplicationResponse } from '../../../core/models/application.models';
import { LoanApplicationStatusResponse } from '../../../core/models/application.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-track-status',
  imports: [RouterLink, DatePipe, DecimalPipe, StatusBadgeComponent],
  templateUrl: './track-status.component.html',
  styleUrl: './track-status.component.css'
})
export class TrackStatusComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private appService = inject(ApplicationService);
  status = signal<LoanApplicationStatusResponse | null>(null);
  applications = signal<LoanApplicationResponse[]>([]);
  loading = signal(true);

  private normalizeStatus(status: string | null | undefined): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  isStatus(status: string): boolean {
    return this.normalizeStatus(this.status()?.currentStatus) === this.normalizeStatus(status);
  }

  displayStatus(status: string | null | undefined): string {
    const normalized = this.normalizeStatus(status);
    const map: Record<string, string> = {
      draft: 'Draft',
      submitted: 'Submitted',
      docspending: 'Docs Pending',
      docsverified: 'Docs Verified',
      underreview: 'Under Review',
      approved: 'Approved',
      rejected: 'Rejected'
    };
    return map[normalized] ?? (status ?? '');
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.appService.getStatus(id).subscribe({
        next: (s) => {
          this.status.set(s);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });

      return;
    }

    this.appService.getMyApplications().subscribe({
      next: (apps) => {
        this.applications.set(apps);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getProgressStep(status: string): number {
    const s = (status || '').toLowerCase().replace(/[\s_-]/g, '');
    if (s === 'draft') return 1;
    if (s === 'submitted') return 2;
    if (s === 'docspending' || s === 'docsverified' || s === 'underreview') return 3;
    if (s === 'approved' || s === 'rejected') return 4;
    return 0;
  }
}
