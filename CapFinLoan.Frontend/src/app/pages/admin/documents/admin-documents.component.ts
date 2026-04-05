import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';
import { DocumentResponse } from '../../../core/models/document.models';

@Component({
  selector: 'app-admin-documents',
  imports: [DatePipe, FormsModule],
  templateUrl: './admin-documents.component.html'
})
export class AdminDocumentsComponent implements OnInit {
  private adminService = inject(AdminService);

  documents = signal<DocumentResponse[]>([]);
  filteredDocuments = signal<DocumentResponse[]>([]);
  loading = signal(true);
  error = signal('');
  successMessage = signal('');
  actionInProgress = signal(false);

  statusFilter = 'All';
  searchQuery = '';

  statusFilters = ['All', 'Pending', 'UnderReview', 'Verified', 'ReuploadRequired'];
  docRemarks: Record<string, string> = {};
  expandedDocId = signal<string | null>(null);

  ngOnInit() {
    this.loadDocuments();
  }

  loadDocuments() {
    this.loading.set(true);
    this.adminService.getAllDocuments().subscribe({
      next: docs => {
        this.documents.set(docs);
        this.applyFilters();
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message || 'Failed to load documents.');
        this.loading.set(false);
      }
    });
  }

  applyFilters() {
    let docs = [...this.documents()];

    if (this.statusFilter !== 'All') {
      docs = docs.filter(d => d.status === this.statusFilter);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      docs = docs.filter(d =>
        d.documentType.toLowerCase().includes(q) ||
        d.fileName.toLowerCase().includes(q) ||
        d.userId.toLowerCase().includes(q)
      );
    }

    this.filteredDocuments.set(docs);
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Verified': return 'bg-green-100 text-green-700 border-green-300';
      case 'ReuploadRequired': return 'bg-amber-100 text-amber-700 border-amber-300';
      case 'UnderReview': return 'bg-blue-100 text-blue-700 border-blue-300';
      default: return 'bg-slate-100 text-slate-600 border-slate-300';
    }
  }

  toggleRemarks(docId: string) {
    this.expandedDocId.set(this.expandedDocId() === docId ? null : docId);
    this.error.set('');
    this.successMessage.set('');
  }

  verifyDocument(docId: string) {
    this.actionInProgress.set(true);
    this.adminService.verifyDocument(docId, { isVerified: true, remarks: '' }).subscribe({
      next: () => {
        this.successMessage.set('Document verified successfully.');
        this.actionInProgress.set(false);
        this.loadDocuments();
        setTimeout(() => this.successMessage.set(''), 3000);
      },
      error: err => {
        this.error.set(err.error?.message || 'Verification failed.');
        this.actionInProgress.set(false);
      }
    });
  }

  requestReupload(docId: string) {
    const remark = (this.docRemarks[docId] || '').trim();
    if (!remark) {
      this.error.set('Please add a remark explaining why re-upload is needed.');
      return;
    }

    this.actionInProgress.set(true);
    this.adminService.verifyDocument(docId, { isVerified: false, remarks: remark }).subscribe({
      next: () => {
        this.successMessage.set('Document marked for re-upload.');
        this.expandedDocId.set(null);
        delete this.docRemarks[docId];
        this.actionInProgress.set(false);
        this.loadDocuments();
        setTimeout(() => this.successMessage.set(''), 3000);
      },
      error: err => {
        this.error.set(err.error?.message || 'Failed to update document.');
        this.actionInProgress.set(false);
      }
    });
  }

  viewDocument(doc: DocumentResponse) {
    this.adminService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 10000);
      },
      error: () => this.error.set('Failed to view document.')
    });
  }

  downloadDocument(doc: DocumentResponse) {
    this.adminService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => this.error.set('Failed to download document.')
    });
  }

  get pendingCount(): number {
    return this.documents().filter(d => d.status === 'Pending').length;
  }
}
