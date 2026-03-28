import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { AdminDashboardResponse } from '../../../core/models/admin.models';

@Component({
  selector: 'app-admin-reports',
  templateUrl: './reports.component.html'
})
export class AdminReportsComponent implements OnInit {
  private adminService = inject(AdminService);

  dashboard = signal<AdminDashboardResponse | null>(null);
  loading = signal(true);

  approvalRate = computed(() => {
    const d = this.dashboard();
    return d && d.totalApplications > 0 ? Math.round((d.approvedCount / d.totalApplications) * 100) : 0;
  });

  rejectionRate = computed(() => {
    const d = this.dashboard();
    return d && d.totalApplications > 0 ? Math.round((d.rejectedCount / d.totalApplications) * 100) : 0;
  });

  ngOnInit() {
    this.adminService.getDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
