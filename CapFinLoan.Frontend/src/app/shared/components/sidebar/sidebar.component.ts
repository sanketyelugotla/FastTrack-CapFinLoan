import { computed, Component, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  role = input.required<'ADMIN' | 'APPLICANT'>();
  auth = inject(AuthService);

  headerTitle = computed(() => this.role() === 'ADMIN' ? 'Cap FinLoan' : 'Cap FinLoan');
  headerSubtitle = computed(() => this.role() === 'ADMIN' ? 'Admin Workspace' : 'Borrower Workspace');
  headerIcon = computed(() => this.role() === 'ADMIN' ? 'admin_panel_settings' : 'account_balance_wallet');

  navItems = computed(() => this.role() === 'ADMIN'
    ? [
      { label: 'Queue', path: '/admin/queue', exact: true, icon: 'format_list_bulleted' },
      { label: 'Documents', path: '/admin/documents', exact: true, icon: 'folder_open' },
      { label: 'Users', path: '/admin/users', exact: false, icon: 'group' },
      { label: 'Reports', path: '/admin/reports', exact: true, icon: 'analytics' }
    ]
    : [
      { label: 'Dashboard', path: '/applicant/dashboard', exact: true, icon: 'dashboard' },
      { label: 'Apply Loan', path: '/applicant/apply', exact: true, icon: 'account_balance' },
      { label: 'My Applications', path: '/applicant/applications', exact: false, icon: 'description' },
      { label: 'My Documents', path: '/applicant/documents', exact: true, icon: 'folder_open' },
      { label: 'Profile', path: '/applicant/profile', exact: true, icon: 'person' }
    ]
  );
}
