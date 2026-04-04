import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApplicationService } from '../../../core/services/application.service';
import { SaveLoanApplicationRequest } from '../../../core/models/application.models';

@Component({
  selector: 'app-apply-loan',
  imports: [FormsModule],
  templateUrl: './apply-loan.component.html',
  styleUrl: './apply-loan.component.css'
})
export class ApplyLoanComponent implements OnInit {
  private appService = inject(ApplicationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  stepLabels = ['Personal', 'Employment', 'Loan', 'Review'];
  currentStep = signal(0);
  saving = signal(false);
  loading = signal(false);
  error = signal('');
  draftId = signal<string | null>(null);
  profileAutoFilled = signal(false);

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
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.draftId.set(id);
      this.loading.set(true);
      this.appService.getById(id).subscribe({
        next: (app) => {
          this.form.personalDetails = { ...app.personalDetails };
          this.form.employmentDetails = { ...app.employmentDetails };
          this.form.loanDetails = { ...app.loanDetails };
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Failed to load application details.');
          this.loading.set(false);
        }
      });
    } else {
      // Auto-fill from saved profile data
      this.loadProfileData();
    }
  }

  private loadProfileData() {
    const saved = localStorage.getItem('capfinloan_profile');
    if (!saved) return;

    try {
      const profile = JSON.parse(saved);
      let filled = false;

      if (profile.personalDetails) {
        const p = profile.personalDetails;
        if (p.firstName) { this.form.personalDetails.firstName = p.firstName; filled = true; }
        if (p.lastName) { this.form.personalDetails.lastName = p.lastName; filled = true; }
        if (p.dateOfBirth) { this.form.personalDetails.dateOfBirth = p.dateOfBirth; filled = true; }
        if (p.gender) { this.form.personalDetails.gender = p.gender; filled = true; }
        if (p.email) { this.form.personalDetails.email = p.email; filled = true; }
        if (p.phone) { this.form.personalDetails.phone = p.phone; filled = true; }
        if (p.addressLine1) { this.form.personalDetails.addressLine1 = p.addressLine1; filled = true; }
        if (p.addressLine2) { this.form.personalDetails.addressLine2 = p.addressLine2; filled = true; }
        if (p.city) { this.form.personalDetails.city = p.city; filled = true; }
        if (p.state) { this.form.personalDetails.state = p.state; filled = true; }
        if (p.postalCode) { this.form.personalDetails.postalCode = p.postalCode; filled = true; }
      }

      if (profile.employmentDetails) {
        const e = profile.employmentDetails;
        if (e.employerName) { this.form.employmentDetails.employerName = e.employerName; filled = true; }
        if (e.employmentType) { this.form.employmentDetails.employmentType = e.employmentType; filled = true; }
        if (e.monthlyIncome) { this.form.employmentDetails.monthlyIncome = e.monthlyIncome; filled = true; }
        if (e.annualIncome) { this.form.employmentDetails.annualIncome = e.annualIncome; filled = true; }
        if (e.existingEmiAmount !== undefined) { this.form.employmentDetails.existingEmiAmount = e.existingEmiAmount; filled = true; }
      }

      if (filled) {
        this.profileAutoFilled.set(true);
        setTimeout(() => this.profileAutoFilled.set(false), 5000);
      }
    } catch { /* ignore corrupt data */ }
  }

  saveDraftAndNext() {
    this.saving.set(true);
    this.error.set('');

    const action = this.draftId()
      ? this.appService.updateDraft(this.draftId()!, this.form)
      : this.appService.createDraft(this.form);

    action.subscribe({
      next: (res) => {
        this.draftId.set(res.id);
        this.saving.set(false);
        this.currentStep.set(this.currentStep() + 1);
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

    // Save final draft then submit
    this.appService.updateDraft(this.draftId()!, this.form).subscribe({
      next: () => {
        this.appService.submit(this.draftId()!).subscribe({
          next: () => {
            this.saving.set(false);
            this.router.navigate(['/applicant/applications']);
          },
          error: (err) => {
            this.saving.set(false);
            this.error.set(err.error?.message || 'Submission failed.');
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
