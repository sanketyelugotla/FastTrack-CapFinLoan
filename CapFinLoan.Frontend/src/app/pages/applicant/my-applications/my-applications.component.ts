import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { LoanApplicationResponse } from '../../../core/models/application.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-my-applications',
  imports: [RouterLink, StatusBadgeComponent, DecimalPipe, DatePipe],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.css'
})
export class MyApplicationsComponent implements OnInit {
  private appService = inject(ApplicationService);
  applications = signal<LoanApplicationResponse[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.loadApplications();
  }

  loadApplications() {
    this.appService.getMyApplications().subscribe({
      next: (apps) => { this.applications.set(apps); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  deleteDraft(id: string) {
    if (confirm('Delete this draft application?')) {
      this.appService.deleteDraft(id).subscribe(() => this.loadApplications());
    }
  }
}
