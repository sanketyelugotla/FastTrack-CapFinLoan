import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { AdminApplicationDetail } from '../../../core/models/admin.models';
import { DocumentResponse } from '../../../core/models/document.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-application-review',
  imports: [RouterLink, FormsModule, StatusBadgeComponent, DatePipe, DecimalPipe],
  templateUrl: './application-review.component.html',
  styleUrl: './application-review.component.css'
})
export class ApplicationReviewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private adminService = inject(AdminService);

  app = signal<AdminApplicationDetail | null>(null);
  documents = signal<DocumentResponse[]>([]);
  loadingDocs = signal(false);
  actionInProgress = signal(false);
  actionError = signal('');
  actionSuccess = signal('');
  remarks = '';
  interestRate: number | null = null;
  sanctionAmount: number | null = null;

  // Per-document inline reupload state
  reuploadExpandedDocId = signal<string | null>(null);
  docRemarks: Record<string, string> = {};

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.adminService.getApplicationById(id).subscribe(a => this.app.set(a));
    this.loadDocuments(id);
  }

  private normalizeStatus(status: string | null | undefined): string {
    return (status ?? '').toLowerCase().replace(/[\s_-]/g, '');
  }

  isStatus(status: string): boolean {
    return this.normalizeStatus(this.app()?.status) === this.normalizeStatus(status);
  }

  canMarkUnderReview(): boolean {
    const normalized = this.normalizeStatus(this.app()?.status);
    return normalized === 'submitted' || normalized === 'docspending' || normalized === 'docsverified';
  }

  canShowDecisionActions(): boolean {
    return !this.isStatus('Approved') && !this.isStatus('Rejected');
  }

  private loadDocuments(applicationId: string) {
    this.loadingDocs.set(true);
    this.adminService.getDocsByApplication(applicationId).subscribe({
      next: d => {
        this.documents.set(d);
        this.loadingDocs.set(false);
      },
      error: () => {
        this.actionError.set('Unable to load applicant documents. Please refresh.');
        this.loadingDocs.set(false);
      }
    });
  }

  private setActionMessage(successMessage: string) {
    this.actionError.set('');
    this.actionSuccess.set(successMessage);
  }

  private setActionError(defaultMessage: string, err: any) {
    this.actionSuccess.set('');
    this.actionError.set(err?.error?.message || defaultMessage);
  }

  updateStatus(status: string) {
    if (!this.app()) {
      return;
    }

    if ((this.normalizeStatus(status) === 'rejected' || this.normalizeStatus(status) === 'docspending') && !this.remarks.trim()) {
      this.actionSuccess.set('');
      this.actionError.set('Please add remarks before rejecting or requesting document re-upload.');
      return;
    }

    this.actionInProgress.set(true);
    this.actionError.set('');
    const id = this.app()!.id;
    const request: any = { targetStatus: status, remarks: this.remarks.trim() };

    // Include interest rate and sanction amount when approving
    if (this.normalizeStatus(status) === 'approved') {
      if (!this.interestRate || this.interestRate <= 0) {
        this.actionError.set('Please specify an interest rate before approving.');
        this.actionInProgress.set(false);
        return;
      }
      request.interestRate = this.interestRate;
      request.sanctionAmount = this.sanctionAmount || this.app()!.requestedAmount;
    }

    this.adminService.updateStatus(id, request).subscribe({
      next: a => {
        this.app.set(a);
        this.remarks = '';
        this.interestRate = null;
        this.sanctionAmount = null;
        this.setActionMessage(`Application moved to ${a.status}.`);
        this.actionInProgress.set(false);
      },
      error: (err) => {
        this.setActionError('Failed to update application status.', err);
        this.actionInProgress.set(false);
      }
    });
  }

  toggleReupload(docId: string) {
    this.reuploadExpandedDocId.set(
      this.reuploadExpandedDocId() === docId ? null : docId
    );
    this.actionError.set('');
    this.actionSuccess.set('');
  }

  verifyDoc(docId: string, isVerified: boolean, remark?: string) {
    if (!this.app()) {
      return;
    }

    const reason = !isVerified ? (remark ?? '').trim() : '';
    if (!isVerified && !reason) {
      this.actionSuccess.set('');
      this.actionError.set('Add a remark before marking a document for re-upload.');
      return;
    }

    this.actionInProgress.set(true);
    this.actionError.set('');
    this.adminService.verifyDocument(docId, { isVerified, remarks: reason }).subscribe({
      next: () => {
        const id = this.app()!.id;
        this.reuploadExpandedDocId.set(null);
        delete this.docRemarks[docId];
        this.loadDocuments(id);
        this.setActionMessage(isVerified ? 'Document verified successfully.' : 'Document marked for re-upload.');
        this.actionInProgress.set(false);
      },
      error: (err) => {
        this.setActionError('Failed to update document verification.', err);
        this.actionInProgress.set(false);
      }
    });
  }

  requestReupload() {
    this.updateStatus('DocsPending');
  }
}
