import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe, SlicePipe } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { AdminDashboardResponse, AdminApplicationSummary } from '../../../core/models/admin.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-admin-reports',
  imports: [DatePipe, DecimalPipe, SlicePipe, StatusBadgeComponent],
  templateUrl: './reports.component.html'
})
export class AdminReportsComponent implements OnInit {
  private adminService = inject(AdminService);

  dashboard = signal<AdminDashboardResponse | null>(null);
  recentApplications = signal<AdminApplicationSummary[]>([]);
  loading = signal(true);
  actionLoading = signal(false);

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

    this.adminService.getQueue().subscribe({
      next: (apps) => this.recentApplications.set(apps),
      error: () => {}
    });
  }

  downloadCsv() {
    this.actionLoading.set(true);
    this.adminService.getQueue().subscribe({
      next: (apps) => {
        this.actionLoading.set(false);
        if (!apps || apps.length === 0) {
          alert('No data available for export.');
          return;
        }

        const headers = ['Application Number', 'Applicant Name', 'Email', 'Phone', 'Requested Amount (₹)', 'Tenure (Months)', 'Status', 'Remarks', 'Submitted Date'];
        const rows = apps.map(app => [
          app.applicationNumber,
          app.applicantName,
          app.email,
          app.phone,
          app.requestedAmount.toLocaleString('en-IN'),
          app.requestedTenureMonths.toString(),
          app.status,
          app.remarks ?? '',
          app.submittedAtUtc ? new Date(app.submittedAtUtc).toLocaleDateString('en-IN') : 'N/A'
        ]);

        const csvContent = [
          headers.join(','),
          ...rows.map(r => r.map(cell => `"${cell || ''}"`).join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `CapFinLoan_Ledger_${new Date().toISOString().split('T')[0]}.csv`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.actionLoading.set(false);
        alert('Failed to fetch data for CSV export.');
      }
    });
  }

  generateFullReport() {
    window.print();
  }
}
