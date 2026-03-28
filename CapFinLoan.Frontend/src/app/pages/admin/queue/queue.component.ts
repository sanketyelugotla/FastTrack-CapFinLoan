import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { AdminApplicationSummary, AdminDashboardResponse } from '../../../core/models/admin.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-admin-queue',
  imports: [RouterLink, StatusBadgeComponent, DecimalPipe],
  templateUrl: './queue.component.html',
  styleUrl: './queue.component.css'
})
export class AdminQueueComponent implements OnInit {
  private adminService = inject(AdminService);
  applications = signal<AdminApplicationSummary[]>([]);
  dashboard = signal<AdminDashboardResponse | null>(null);
  loading = signal(true);
  activeFilter = signal('');

  tabs = [
    { label: 'All', value: '' },
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Docs Pending', value: 'DocsPending' },
    { label: 'Under Review', value: 'UnderReview' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Rejected', value: 'Rejected' }
  ];

  ngOnInit() {
    this.adminService.getDashboard().subscribe({
      next: (data) => this.dashboard.set(data)
    });
    this.filter('');
  }

  filter(status: string) {
    this.activeFilter.set(status);
    this.loading.set(true);
    this.adminService.getQueue(status || undefined).subscribe({
      next: (apps) => { this.applications.set(apps); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  initials(name: string) {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }
}
