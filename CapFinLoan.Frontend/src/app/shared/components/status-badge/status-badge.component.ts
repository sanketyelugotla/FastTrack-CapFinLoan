import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  status = input.required<string>();

  private normalizeStatus(status: string): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  label() {
    const map: Record<string, string> = {
      draft: 'Draft',
      submitted: 'Submitted',
      docspending: 'Docs Pending',
      docsverified: 'Docs Verified',
      underreview: 'Under Review',
      approved: 'Approved',
      rejected: 'Rejected'
    };
    return map[this.normalizeStatus(this.status())] ?? this.status();
  }

  badgeClass() {
    const map: Record<string, string> = {
      draft: 'badge badge-draft',
      submitted: 'badge badge-submitted',
      docspending: 'badge badge-docs-pending',
      docsverified: 'badge badge-docs-pending',
      underreview: 'badge badge-under-review',
      approved: 'badge badge-approved',
      rejected: 'badge badge-rejected'
    };
    return map[this.normalizeStatus(this.status())] ?? 'badge badge-draft';
  }
}
