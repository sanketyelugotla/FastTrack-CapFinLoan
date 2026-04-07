import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { DatePipe, UpperCasePipe } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { UserSummary } from '../../../core/models/auth.models';

@Component({
  selector: 'app-admin-users',
  imports: [DatePipe, UpperCasePipe],
  templateUrl: './users.component.html'
})
export class AdminUsersComponent implements OnInit {
  private adminService = inject(AdminService);
  users = signal<UserSummary[]>([]);
  loading = signal(true);
  showFilters = signal(false);
  roleFilter = signal<'ALL' | 'ADMIN' | 'APPLICANT'>('ALL');
  statusFilter = signal<'ALL' | 'ACTIVE' | 'INACTIVE'>('ALL');

  // Computed properties for Dashboard KPIs
  totalUsers = computed(() => this.users().length);
  activeUsers = computed(() => this.users().filter(u => u.isActive).length);
  adminUsers = computed(() => this.users().filter(u => u.role === 'ADMIN').length);
  inactiveUsers = computed(() => this.users().filter(u => !u.isActive).length);
  filteredUsers = computed(() => this.users().filter(user => {
    const roleMatches = this.roleFilter() === 'ALL' || user.role === this.roleFilter();
    const statusMatches = this.statusFilter() === 'ALL'
      || (this.statusFilter() === 'ACTIVE' && user.isActive)
      || (this.statusFilter() === 'INACTIVE' && !user.isActive);

    return roleMatches && statusMatches;
  }));

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.adminService.getUsers().subscribe({
      next: (u) => { this.users.set(u); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  toggleStatus(user: UserSummary) {
    this.adminService.updateUserStatus(user.id, !user.isActive).subscribe(() => this.loadUsers());
  }

  toggleFilters() {
    this.showFilters.update(value => !value);
  }

  setRoleFilter(role: string) {
    this.roleFilter.set(role as 'ALL' | 'ADMIN' | 'APPLICANT');
  }

  setStatusFilter(status: string) {
    this.statusFilter.set(status as 'ALL' | 'ACTIVE' | 'INACTIVE');
  }

  // Simple UI helper
  getInitials(email: string) {
    return email ? email.substring(0, 2).toUpperCase() : 'US';
  }
}
