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

  form: SaveLoanApplicationRequest = {
    personalDetails: { firstName: 'Sanket', lastName: 'Yelugotla', dateOfBirth: '2005-06-02', gender: 'Male', email: 'sanket@gmail.com', phone: '9550572255', addressLine1: 'Sardar Pg', addressLine2: 'Law gate', city: 'Phagwara', state: 'Punjab', postalCode: '144411' },
    employmentDetails: { employerName: 'Sanket Yelugotla', employmentType: 'Salaried', monthlyIncome: 50000, annualIncome: 600000, existingEmiAmount: 10000 },
    loanDetails: { requestedAmount: 100000, requestedTenureMonths: 12, loanPurpose: 'Personel', remarks: 'Nothing' }
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
    }
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
