import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { LoanApplicationResponse } from '../../../core/models/application.models';
import { LoanApplicationStatusResponse } from '../../../core/models/application.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-track-status',
  imports: [RouterLink, DatePipe, StatusBadgeComponent],
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
}
