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
