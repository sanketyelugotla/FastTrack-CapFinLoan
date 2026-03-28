import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  status = input.required<string>();

  label() {
    const map: Record<string, string> = {
      'Draft': 'Draft',
      'Submitted': 'Submitted',
      'DocsPending': 'Docs Pending',
      'UnderReview': 'Under Review',
      'Approved': 'Approved',
      'Rejected': 'Rejected'
    };
    return map[this.status()] ?? this.status();
  }

  badgeClass() {
    const map: Record<string, string> = {
      'Draft': 'badge badge-draft',
      'Submitted': 'badge badge-submitted',
      'DocsPending': 'badge badge-docs-pending',
      'UnderReview': 'badge badge-under-review',
      'Approved': 'badge badge-approved',
      'Rejected': 'badge badge-rejected'
    };
    return map[this.status()] ?? 'badge badge-draft';
  }
}
