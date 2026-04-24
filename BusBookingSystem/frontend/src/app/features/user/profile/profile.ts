import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  fb = inject(FormBuilder);
  auth = inject(AuthService);
  api = inject(ApiService);

  profileForm!: FormGroup;
  operatorForm!: FormGroup;

  user: any;
  loadingProfile = false;
  loadingOperator = false;
  message = '';
  
  showOperatorModal = false;

  ngOnInit() {
    this.user = this.auth.currentUser();
    this.profileForm = this.fb.group({
      name: [this.user?.name || '', Validators.required],
      phone: [this.user?.phone || '', Validators.required],
      age: [this.user?.age || '', [Validators.required, Validators.min(18)]],
      gender: [this.user?.gender || '', Validators.required]
    });

    this.operatorForm = this.fb.group({
      companyName: ['', Validators.required],
      contactNumber: ['', Validators.required],
      operatingLocation: ['', Validators.required]
    });
  }

  updateProfile() {
    if (this.profileForm.valid) {
      this.loadingProfile = true;
      this.auth.updateProfile(this.profileForm.value).subscribe({
        next: () => {
          this.loadingProfile = false;
          this.message = 'Profile updated successfully.';
          setTimeout(() => this.message = '', 3000);
        },
        error: () => {
          this.loadingProfile = false;
          alert('Failed to update profile.');
        }
      });
    }
  }

  registerAsOperator() {
    if (this.operatorForm.valid) {
      this.loadingOperator = true;
      this.api.registerOperator(this.operatorForm.value).subscribe({
        next: () => {
          this.loadingOperator = false;
          this.showOperatorModal = false;
          alert('Operator registration submitted. Please wait for admin approval.');
        },
        error: (err) => {
          this.loadingOperator = false;
          alert(err.error?.message || err.error || 'Failed to register as operator.');
        }
      });
    }
  }
}
