import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  auth = inject(AuthService);
  private fb = inject(FormBuilder);

  profileForm!: FormGroup;
  isEditing = false;
  successMessage = '';
  activeSection: 'personal' | 'employment' = 'personal';

  ngOnInit() {
    const user = this.auth.currentUser();
    const saved = this.loadSavedProfile();

    this.profileForm = this.fb.group({
      // Personal Details
      firstName: [saved?.personalDetails?.firstName || user?.name?.split(' ')[0] || '', [Validators.required, Validators.minLength(2)]],
      lastName: [saved?.personalDetails?.lastName || user?.name?.split(' ').slice(1).join(' ') || ''],
      email: [{ value: user?.email || '', disabled: true }],
      phone: [saved?.personalDetails?.phone || '', [Validators.pattern('^[0-9+() -]*$')]],
      dateOfBirth: [saved?.personalDetails?.dateOfBirth || ''],
      gender: [saved?.personalDetails?.gender || ''],
      addressLine1: [saved?.personalDetails?.addressLine1 || ''],
      addressLine2: [saved?.personalDetails?.addressLine2 || ''],
      city: [saved?.personalDetails?.city || ''],
      state: [saved?.personalDetails?.state || ''],
      postalCode: [saved?.personalDetails?.postalCode || ''],

      // Employment Details
      employerName: [saved?.employmentDetails?.employerName || ''],
      employmentType: [saved?.employmentDetails?.employmentType || ''],
      monthlyIncome: [saved?.employmentDetails?.monthlyIncome || null],
      annualIncome: [saved?.employmentDetails?.annualIncome || null],
      existingEmiAmount: [saved?.employmentDetails?.existingEmiAmount || 0]
    });
  }

  private loadSavedProfile(): any {
    try {
      const data = localStorage.getItem('capfinloan_profile');
      return data ? JSON.parse(data) : null;
    } catch { return null; }
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.successMessage = '';
  }

  saveProfile() {
    if (this.profileForm.valid) {
      const v = this.profileForm.getRawValue();

      // Save structured profile to localStorage for auto-fill
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

      localStorage.setItem('capfinloan_profile', JSON.stringify(profileData));
      this.successMessage = 'Profile saved! Your details will auto-fill loan applications.';
      this.isEditing = false;

      setTimeout(() => {
        this.successMessage = '';
      }, 5000);
    }
  }
}
