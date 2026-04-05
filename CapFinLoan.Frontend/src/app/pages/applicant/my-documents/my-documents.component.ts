import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentResponse } from '../../../core/models/document.models';

@Component({
  selector: 'app-my-documents',
  imports: [DatePipe],
  templateUrl: './my-documents.component.html'
})
export class MyDocumentsComponent implements OnInit {
  private docService = inject(DocumentService);

  documents = signal<DocumentResponse[]>([]);
  loading = signal(true);
  error = signal('');
  uploadSuccess = signal('');

  docTypes = [
    { value: 'IdProof', label: 'ID Proof', description: 'Aadhaar, PAN, Passport, or Voter ID', icon: 'badge' },
    { value: 'AddressProof', label: 'Address Proof', description: 'Utility bill, Aadhaar, or Passport', icon: 'home' },
    { value: 'IncomeProof', label: 'Income Proof', description: 'Salary slips (last 3 months) or ITR', icon: 'payments' },
    { value: 'BankStatement', label: 'Bank Statement', description: 'Last 6 months bank statement', icon: 'account_balance' }
  ];

  ngOnInit() {
    this.loadDocuments();
  }

  loadDocuments() {
    this.loading.set(true);
    this.docService.getMyDocuments().subscribe({
      next: docs => {
        this.documents.set(docs);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load documents.');
        this.loading.set(false);
      }
    });
  }

  getDocForType(type: string): DocumentResponse | undefined {
    return this.documents().find(d => d.documentType === type);
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Verified': return 'bg-green-100 text-green-700 border-green-300';
      case 'ReuploadRequired': return 'bg-amber-100 text-amber-700 border-amber-300';
      case 'UnderReview': return 'bg-blue-100 text-blue-700 border-blue-300';
      default: return 'bg-slate-100 text-slate-600 border-slate-300';
    }
  }

  uploadFile(event: Event, docType: string) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.error.set('');
    this.uploadSuccess.set('');

    // Upload without applicationId — standalone KYC document (use Guid.Empty equivalent)
    this.docService.upload('00000000-0000-0000-0000-000000000000', docType, file).subscribe({
      next: () => {
        this.uploadSuccess.set(`${docType} uploaded successfully!`);
        this.loadDocuments();
        setTimeout(() => this.uploadSuccess.set(''), 3000);
      },
      error: (err) => this.error.set(err.error?.message || 'Upload failed.')
    });
  }

  replaceFile(event: Event, documentId: string, docType: string) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.error.set('');
    this.docService.replace(documentId, file, docType).subscribe({
      next: () => {
        this.uploadSuccess.set('Document replaced successfully!');
        this.loadDocuments();
        setTimeout(() => this.uploadSuccess.set(''), 3000);
      },
      error: (err) => this.error.set(err.error?.message || 'Failed to replace document.')
    });
  }
}
