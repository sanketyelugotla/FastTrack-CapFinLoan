import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentResponse } from '../../../core/models/document.models';

@Component({
  selector: 'app-documents',
  imports: [RouterLink, DatePipe],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.css'
})
export class DocumentsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private docService = inject(DocumentService);

  applicationId = '';
  documents = signal<DocumentResponse[]>([]);
  error = signal('');

  docTypes = [
    { value: 'IdProof', label: 'ID Proof', description: 'Aadhaar, PAN, Passport, or Voter ID' },
    { value: 'AddressProof', label: 'Address Proof', description: 'Utility bill, Aadhaar, or Passport' },
    { value: 'IncomeProof', label: 'Income Proof', description: 'Salary slips (last 3 months) or ITR' },
    { value: 'BankStatement', label: 'Bank Statement', description: 'Last 6 months bank statement' }
  ];

  ngOnInit() {
    this.applicationId = this.route.snapshot.paramMap.get('id')!;
    this.loadDocuments();
  }

  loadDocuments() {
    this.docService.getByApplicationId(this.applicationId).subscribe({
      next: docs => this.documents.set(docs),
      error: (err) => this.error.set(err.error?.message || 'Failed to load documents.')
    });
  }

  getDocForType(type: string): DocumentResponse | undefined {
    return this.documents().find(d => d.documentType === type);
  }

  uploadFile(event: Event, docType: string) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.docService.upload(this.applicationId, docType, file).subscribe({
      next: () => {
        this.error.set('');
        this.loadDocuments();
      },
      error: (err) => this.error.set(err.error?.message || 'Upload failed.')
    });
  }

  replaceFile(event: Event, documentId: string, docType: string) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.docService.replace(documentId, file, docType).subscribe({
      next: () => {
        this.error.set('');
        this.loadDocuments();
      },
      error: (err) => this.error.set(err.error?.message || 'Failed to replace document.')
    });
  }
}
