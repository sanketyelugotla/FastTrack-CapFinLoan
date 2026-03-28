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

  // Computed properties for Dashboard KPIs
  totalUsers = computed(() => this.users().length);
  activeUsers = computed(() => this.users().filter(u => u.isActive).length);
  adminUsers = computed(() => this.users().filter(u => u.role === 'ADMIN').length);
  inactiveUsers = computed(() => this.users().filter(u => !u.isActive).length);

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

  // Simple UI helper
  getInitials(email: string) {
    return email ? email.substring(0, 2).toUpperCase() : 'US';
  }
}
