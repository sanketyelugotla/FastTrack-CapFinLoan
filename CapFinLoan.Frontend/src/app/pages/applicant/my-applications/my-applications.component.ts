import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationService } from '../../../core/services/application.service';
import { LoanApplicationResponse } from '../../../core/models/application.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-my-applications',
  imports: [RouterLink, StatusBadgeComponent, DecimalPipe, DatePipe, FormsModule],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.css'
})
export class MyApplicationsComponent implements OnInit {
  private appService = inject(ApplicationService);
  applications = signal<LoanApplicationResponse[]>([]);
  loading = signal(true);

  private normalizeStatus(status: string): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  // Filter state
  statusFilter = signal('All');
  searchQuery = signal('');

  readonly statusOptions = ['All', 'Draft', 'Submitted', 'Docs Pending', 'Docs Verified', 'Under Review', 'Approved', 'Rejected'];

  filteredApplications = computed(() => {
    let apps = this.applications();
    const status = this.statusFilter();
    const query = this.searchQuery().toLowerCase().trim();

    if (status !== 'All') {
      const target = this.normalizeStatus(status);
      apps = apps.filter(a => this.normalizeStatus(a.status) === target);
    }
    if (query) {
      apps = apps.filter(a =>
        a.applicationNumber.toLowerCase().includes(query) ||
        (a.loanDetails.loanPurpose || '').toLowerCase().includes(query)
      );
    }
    return apps;
  });

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

  onStatusChange(value: string) {
    this.statusFilter.set(value);
  }

  onSearchChange(value: string) {
    this.searchQuery.set(value);
  }
}
