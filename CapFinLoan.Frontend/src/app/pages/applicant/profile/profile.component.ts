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

  ngOnInit() {
    const user = this.auth.currentUser();
    this.profileForm = this.fb.group({
      name: [user?.name || '', [Validators.required, Validators.minLength(2)]],
      email: [{ value: user?.email || '', disabled: true }],
      phone: ['', [Validators.pattern('^[0-9+() -]*$')]],
      address: ['']
    });
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.successMessage = '';
  }

  saveProfile() {
    if (this.profileForm.valid) {
      // In a real application, we would call an API service here to update.
      // E.g., this.auth.updateProfile(this.profileForm.value).subscribe(...)
      this.successMessage = 'Profile updated successfully!';
      this.isEditing = false;
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        this.successMessage = '';
      }, 3000);
    }
  }
}
