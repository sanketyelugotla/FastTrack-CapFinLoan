import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { DocumentService } from '../../../core/services/document.service';
import { LoanApplicationResponse, LoanApplicationStatusResponse } from '../../../core/models/application.models';
import { DocumentResponse } from '../../../core/models/document.models';

@Component({
  selector: 'app-application-detail',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './application-detail.component.html'
})
export class ApplicationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private appService = inject(ApplicationService);
  private docService = inject(DocumentService);

  app = signal<LoanApplicationResponse | null>(null);
  tracking = signal<LoanApplicationStatusResponse | null>(null);
  documents = signal<DocumentResponse[]>([]);

  private normalizeStatus(status: string | null | undefined): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  displayStatus(status: string | null | undefined): string {
    const normalized = this.normalizeStatus(status);
    const map: Record<string, string> = {
      draft: 'Draft',
      submitted: 'Submitted',
      docspending: 'Docs Pending',
      docsverified: 'Docs Verified',
      underreview: 'Under Review',
      approved: 'Approved',
      rejected: 'Rejected'
    };
    return map[normalized] ?? (status ?? '');
  }

  hasPendingDocuments = computed(() =>
    this.documents().some(doc => doc.status === 'ReuploadRequired')
  );

  allDocumentsVerified = computed(() =>
    this.documents().length > 0 && this.documents().every(doc => doc.status === 'Verified')
  );

  canManageDocuments = computed(() =>
    !this.isDecisionReady() && this.documents().length > 0
  );

  // Computed signals for Timeline mapping
  isSubmitted = computed(() => {
    const status = this.normalizeStatus(this.app()?.status);
    return !!this.app()?.submittedAtUtc ||
      status === 'submitted' ||
      status === 'docspending' ||
      status === 'docsverified' ||
      status === 'underreview' ||
      status === 'approved' ||
      status === 'rejected';
  });

  isDocsVerified = computed(() => this.allDocumentsVerified());

  isUnderReview = computed(() => this.normalizeStatus(this.app()?.status) === 'underreview');
  isDecisionReady = computed(() => ['approved', 'rejected'].includes(this.normalizeStatus(this.app()?.status)));

  isStatus(status: string): boolean {
    return this.normalizeStatus(this.app()?.status) === this.normalizeStatus(status);
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;

    // Load Application Details
    this.appService.getById(id).subscribe(a => this.app.set(a));

    // Load Status Timeline
    this.appService.getStatus(id).subscribe(t => this.tracking.set(t));

    // Load Uploaded Documents
    this.docService.getByApplicationId(id).subscribe(d => this.documents.set(d));
  }
}
