import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, DecimalPipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ApplicationService } from '../../../core/services/application.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, CommonModule, DecimalPipe],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  auth = inject(AuthService);
  private appService = inject(ApplicationService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  profileForm!: FormGroup;
  isEditing = false;
  successMessage = '';
  activeSection: 'personal' | 'employment' = 'personal';

  ngOnInit() {
    const user = this.auth.currentUser();

    this.profileForm = this.fb.group({
      // Personal Details
      firstName: [user?.name?.split(' ')[0] || '', [Validators.required, Validators.minLength(2)]],
      lastName: [user?.name?.split(' ').slice(1).join(' ') || ''],
      email: [{ value: user?.email || '', disabled: true }],
      phone: ['', [Validators.pattern('^[0-9+() -]*$')]],
      dateOfBirth: [''],
      gender: [''],
      addressLine1: [''],
      addressLine2: [''],
      city: [''],
      state: [''],
      postalCode: [''],

      // Employment Details
      employerName: [''],
      employmentType: [''],
      monthlyIncome: [null],
      annualIncome: [null],
      existingEmiAmount: [0]
    });

    this.appService.getProfile().subscribe({
      next: (profile) => {
        this.profileForm.patchValue({
          firstName: profile.personalDetails.firstName || this.profileForm.get('firstName')?.value,
          lastName: profile.personalDetails.lastName || this.profileForm.get('lastName')?.value,
          email: profile.personalDetails.email || user?.email || '',
          phone: profile.personalDetails.phone,
          dateOfBirth: this.normalizeDateForInput(profile.personalDetails.dateOfBirth),
          gender: profile.personalDetails.gender,
          addressLine1: profile.personalDetails.addressLine1,
          addressLine2: profile.personalDetails.addressLine2,
          city: profile.personalDetails.city,
          state: profile.personalDetails.state,
          postalCode: profile.personalDetails.postalCode,
          employerName: profile.employmentDetails.employerName,
          employmentType: profile.employmentDetails.employmentType,
          monthlyIncome: profile.employmentDetails.monthlyIncome,
          annualIncome: profile.employmentDetails.annualIncome,
          existingEmiAmount: profile.employmentDetails.existingEmiAmount
        });

        this.cdr.detectChanges();
      }
    });
  }

  getFormattedDateOfBirth(): string {
    const value = this.profileForm?.get('dateOfBirth')?.value as string | null;
    const normalized = this.normalizeDateForInput(value);

    if (!normalized) {
      return 'Not provided';
    }

    const [year, month, day] = normalized.split('-').map(Number);
    const monthName = new Date(Date.UTC(year, month - 1, day)).toLocaleString('en-US', {
      month: 'long',
      timeZone: 'UTC'
    });

    return `${this.getOrdinalDay(day)} ${monthName} ${year}`;
  }

  private normalizeDateForInput(value: string | null | undefined): string {
    if (!value) {
      return '';
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
      return '';
    }

    return parsed.toISOString().slice(0, 10);
  }

  private getOrdinalDay(day: number): string {
    const mod100 = day % 100;
    if (mod100 >= 11 && mod100 <= 13) {
      return `${day}th`;
    }

    switch (day % 10) {
      case 1:
        return `${day}st`;
      case 2:
        return `${day}nd`;
      case 3:
        return `${day}rd`;
      default:
        return `${day}th`;
    }
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.successMessage = '';
  }

  saveProfile() {
    if (this.profileForm.valid) {
      const v = this.profileForm.getRawValue();

      const profileData = {
        personalDetails: {
          firstName: v.firstName,
          lastName: v.lastName,
          dateOfBirth: v.dateOfBirth,
          gender: v.gender,
          email: v.email,
          phone: v.phone,
          addressLine1: v.addressLine1,
          addressLine2: v.addressLine2,
          city: v.city,
          state: v.state,
          postalCode: v.postalCode
        },
        employmentDetails: {
          employerName: v.employerName,
          employmentType: v.employmentType,
          monthlyIncome: v.monthlyIncome,
          annualIncome: v.annualIncome,
          existingEmiAmount: v.existingEmiAmount
        }
      };

      this.appService.saveProfile(profileData).subscribe({
        next: () => {
          this.successMessage = 'Profile saved! Your details will auto-fill loan applications.';
          this.isEditing = false;

          setTimeout(() => {
            this.successMessage = '';
          }, 5000);
        }
      });
    }
  }
}
