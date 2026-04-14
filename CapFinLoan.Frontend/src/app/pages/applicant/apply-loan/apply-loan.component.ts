import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService } from '../../../core/services/application.service';
import { DocumentService } from '../../../core/services/document.service';
import { SaveLoanApplicationRequest } from '../../../core/models/application.models';
import { DocumentResponse } from '../../../core/models/document.models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-apply-loan',
  imports: [FormsModule],
  templateUrl: './apply-loan.component.html',
  styleUrl: './apply-loan.component.css'
})
export class ApplyLoanComponent implements OnInit {
  private appService = inject(ApplicationService);
  private docService = inject(DocumentService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  stepLabels = ['Personal', 'Employment', 'Loan', 'Documents', 'Review'];
  currentStep = signal(0);
  saving = signal(false);
  loading = signal(false);
  error = signal('');
  draftId = signal<string | null>(null);
  appStatus = signal<string>('Draft');
  profileAutoFilled = signal(false);

  /** True when the loaded application is no longer editable */
  isReadOnly = computed(() => {
    const s = this.appStatus();
    return s !== 'Draft';
  });

  // Document state
  documents = signal<DocumentResponse[]>([]);
  genericDocuments = signal<DocumentResponse[]>([]);
  docsLoading = signal(false);
  uploadSuccess = signal('');
  docTypes = [
    { value: 'IdProof', label: 'ID Proof', description: 'Aadhaar, PAN, Passport, or Voter ID', icon: 'badge' },
    { value: 'AddressProof', label: 'Address Proof', description: 'Utility bill, Aadhaar, or Passport', icon: 'home' },
    { value: 'IncomeProof', label: 'Income Proof', description: 'Salary slips (last 3 months) or ITR', icon: 'payments' },
    { value: 'BankStatement', label: 'Bank Statement', description: 'Last 6 months bank statement', icon: 'account_balance' }
  ];

  loanPurposes = [
    'Home Loan',
    'Education Loan',
    'Personal Loan',
    'Vehicle Loan',
    'Business Loan',
    'Medical Loan',
    'Debt Consolidation',
    'Agriculture Loan',
    'Gold Loan',
    'Other'
  ];

  form: SaveLoanApplicationRequest = {
    personalDetails: { firstName: '', lastName: '', dateOfBirth: null, gender: '', email: '', phone: '', addressLine1: '', addressLine2: '', city: '', state: '', postalCode: '' },
    employmentDetails: { employerName: '', employmentType: '', monthlyIncome: null, annualIncome: null, existingEmiAmount: 0 },
    loanDetails: { requestedAmount: 0, requestedTenureMonths: 0, loanPurpose: '', remarks: '' }
  };

  ngOnInit() {
    this.docService.getMyDocuments().subscribe({
      next: docs => {
        this.genericDocuments.set(docs.filter(d => d.applicationId === '00000000-0000-0000-0000-000000000000'));
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.draftId.set(id);
      this.loading.set(true);
      this.appService.getById(id).subscribe({
        next: (app) => {
          this.form.personalDetails = {
            ...app.personalDetails,
            dateOfBirth: this.normalizeDateForInput(app.personalDetails.dateOfBirth)
          };
          this.form.employmentDetails = { ...app.employmentDetails };
          this.form.loanDetails = { ...app.loanDetails };
          this.appStatus.set(app.status);
          this.loading.set(false);
          this.loadDocuments(id);
        },
        error: () => {
          this.error.set('Failed to load application details.');
          this.loading.set(false);
        }
      });
    } else {
      this.loadProfileData();
    }
  }

  loadDocuments(appId: string) {
    this.docsLoading.set(true);
    this.docService.getByApplicationId(appId).subscribe({
      next: docs => {
        this.documents.set(docs);
        this.docsLoading.set(false);
      },
      error: () => this.docsLoading.set(false)
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

  // ─── Validation ────────────────────────────────────────────────────────────

  /** Returns an error string if the current step is invalid, empty string if OK */
  validateStep(): string {
    const p = this.form.personalDetails;
    const e = this.form.employmentDetails;
    const l = this.form.loanDetails;

    switch (this.currentStep()) {
      case 0: {
        if (!p.firstName?.trim()) return 'First name is required.';
        if (!p.lastName?.trim()) return 'Last name is required.';
        if (!p.email?.trim()) return 'Email address is required.';
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(p.email)) return 'Please enter a valid email address.';
        if (!p.phone?.trim()) return 'Phone number is required.';
        if (!p.dateOfBirth) return 'Date of birth is required.';
        if (!p.gender) return 'Gender is required.';
        if (!p.addressLine1?.trim()) return 'Address line 1 is required.';
        if (!p.city?.trim()) return 'City is required.';
        if (!p.state?.trim()) return 'State is required.';
        if (!p.postalCode?.trim()) return 'Postal code is required.';
        return '';
      }
      case 1: {
        if (!e.employerName?.trim()) return 'Employer name is required.';
        if (!e.employmentType) return 'Employment type is required.';
        if (!e.monthlyIncome || e.monthlyIncome <= 0) return 'Monthly income must be greater than 0.';
        if (!e.annualIncome || e.annualIncome <= 0) return 'Annual income must be greater than 0.';
        if (e.existingEmiAmount !== undefined && e.existingEmiAmount < 0) return 'Existing EMI cannot be negative.';
        if (e.monthlyIncome <= (e.existingEmiAmount ?? 0)) return 'Monthly income must be greater than existing EMI obligations.';
        return '';
      }
      case 2: {
        if (!l.loanPurpose) return 'Loan purpose is required.';
        if (!l.requestedAmount || l.requestedAmount < 10000) return 'Requested amount must be at least ₹10,000.';
        if (l.requestedAmount > 5000000) return 'Requested amount cannot exceed ₹50,00,000.';
        if (!l.requestedTenureMonths || l.requestedTenureMonths < 6) return 'Loan tenure must be at least 6 months.';
        if (l.requestedTenureMonths > 360) return 'Loan tenure cannot exceed 360 months.';
        return '';
      }
      default:
        return '';
    }
  }

  // ─── Upload ────────────────────────────────────────────────────────────────

  uploadFile(event: Event, docType: string) {
    if (this.isReadOnly()) return;
    const file = (event.target as HTMLInputElement).files?.[0];
    const appId = this.draftId();
    if (!file || !appId) return;

    this.error.set('');
    this.uploadSuccess.set('');

    this.docService.upload(appId, docType, file).subscribe({
      next: () => {
        this.uploadSuccess.set(`${docType} uploaded successfully!`);
        this.loadDocuments(appId);
        setTimeout(() => this.uploadSuccess.set(''), 3000);
      },
      error: (err) => this.error.set(err.error?.message || 'Upload failed.')
    });
  }

  replaceFile(event: Event, documentId: string, docType: string) {
    if (this.isReadOnly()) return;
    const file = (event.target as HTMLInputElement).files?.[0];
    const appId = this.draftId();
    if (!file || !appId) return;

    this.error.set('');
    this.docService.replace(documentId, file, docType).subscribe({
      next: () => {
        this.uploadSuccess.set('Document replaced successfully!');
        this.loadDocuments(appId);
        setTimeout(() => this.uploadSuccess.set(''), 3000);
      },
      error: (err) => this.error.set(err.error?.message || 'Failed to replace document.')
    });
  }

  getGenericDocForType(type: string): DocumentResponse | undefined {
    return this.genericDocuments().find(d => d.documentType === type);
  }

  linkExistingDoc(sourceDocId: string, docType: string) {
    if (this.isReadOnly()) return;
    const appId = this.draftId();
    if (!appId) return;

    this.error.set('');
    this.uploadSuccess.set('');
    this.docsLoading.set(true);

    this.docService.linkDocument(sourceDocId, appId).subscribe({
      next: () => {
        this.uploadSuccess.set(`${docType} linked successfully!`);
        this.loadDocuments(appId);
        setTimeout(() => this.uploadSuccess.set(''), 3000);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Linking failed.');
        this.docsLoading.set(false);
      }
    });
  }

  private loadProfileData() {
    this.appService.getProfile().subscribe({
      next: (profile) => {
        let filled = false;
        const p = profile.personalDetails;
        const e = profile.employmentDetails;

        if (p.firstName) { this.form.personalDetails.firstName = p.firstName; filled = true; }
        if (p.lastName) { this.form.personalDetails.lastName = p.lastName; filled = true; }
        if (p.dateOfBirth) { this.form.personalDetails.dateOfBirth = this.normalizeDateForInput(p.dateOfBirth); filled = true; }
        if (p.gender) { this.form.personalDetails.gender = p.gender; filled = true; }
        if (p.email) { this.form.personalDetails.email = p.email; filled = true; }
        if (p.phone) { this.form.personalDetails.phone = p.phone; filled = true; }
        if (p.addressLine1) { this.form.personalDetails.addressLine1 = p.addressLine1; filled = true; }
        if (p.addressLine2) { this.form.personalDetails.addressLine2 = p.addressLine2; filled = true; }
        if (p.city) { this.form.personalDetails.city = p.city; filled = true; }
        if (p.state) { this.form.personalDetails.state = p.state; filled = true; }
        if (p.postalCode) { this.form.personalDetails.postalCode = p.postalCode; filled = true; }

        if (e.employerName) { this.form.employmentDetails.employerName = e.employerName; filled = true; }
        if (e.employmentType) { this.form.employmentDetails.employmentType = e.employmentType; filled = true; }
        if (e.monthlyIncome) { this.form.employmentDetails.monthlyIncome = e.monthlyIncome; filled = true; }
        if (e.annualIncome) { this.form.employmentDetails.annualIncome = e.annualIncome; filled = true; }
        if (e.existingEmiAmount !== undefined) { this.form.employmentDetails.existingEmiAmount = e.existingEmiAmount; filled = true; }

        if (filled) {
          this.profileAutoFilled.set(true);
          setTimeout(() => this.profileAutoFilled.set(false), 5000);
        }
      }
    });
  }

  private normalizeDateForInput(value: string | null | undefined): string | null {
    if (!value) {
      return null;
    }

    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return value;
    }

    const datePart = value.slice(0, 10);
    if (/^\d{4}-\d{2}-\d{2}$/.test(datePart)) {
      return datePart;
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return null;
    }

    return parsed.toISOString().slice(0, 10);
  }

  // ─── Navigation ────────────────────────────────────────────────────────────

  saveDraftAndNext() {
    this.error.set('');

    // Validate current step before calling the API
    const validationError = this.validateStep();
    if (validationError) {
      this.error.set(validationError);
      return;
    }

    this.saving.set(true);

    if (this.currentStep() === 3 && this.draftId()) {
      this.loadDocuments(this.draftId()!);
      this.saving.set(false);
      this.currentStep.set(this.currentStep() + 1);
      return;
    }

    const action = this.draftId()
      ? this.appService.updateDraft(this.draftId()!, this.form)
      : this.appService.createDraft(this.form);

    action.subscribe({
      next: (res) => {
        this.draftId.set(res.id);
        this.saving.set(false);
        this.currentStep.set(this.currentStep() + 1);
        if (this.currentStep() === 3) {
          this.loadDocuments(res.id);
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.message || 'Failed to save draft.');
      }
    });
  }

  submitApplication() {
    if (!this.draftId()) return;
    this.saving.set(true);
    this.error.set('');

    this.appService.updateDraft(this.draftId()!, this.form).subscribe({
      next: () => {
        this.appService.submit(this.draftId()!).subscribe({
          next: () => {
            this.saving.set(false);
            this.router.navigate(['/applicant/applications']);
          },
          error: (err) => {
            this.saving.set(false);
            this.error.set(err.error?.message || 'Submission failed. Please check all required fields.');
          }
        });
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.message || 'Failed to save draft.');
      }
    });
  }
}
