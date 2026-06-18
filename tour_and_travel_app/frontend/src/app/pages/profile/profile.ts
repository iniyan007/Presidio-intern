import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/user.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  userService = inject(UserService);
  private fb = inject(FormBuilder);

  isEditing = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  profileForm: FormGroup;

  constructor() {
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      phone: ['', [Validators.pattern('^[6-9][0-9]{9}$')]]
    });
  }

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.isLoading.set(true);
    this.userService.loadProfile().subscribe({
      next: (profile) => {
        this.isLoading.set(false);
        this.profileForm.patchValue({
          fullName: profile.fullName,
          phone: profile.phone || ''
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('Failed to load profile. Please try again.');
      }
    });
  }

  toggleEdit() {
    this.isEditing.set(!this.isEditing());
    if (!this.isEditing()) {
      // Revert changes if cancelled
      const profile = this.userService.userProfile();
      if (profile) {
        this.profileForm.patchValue({
          fullName: profile.fullName,
          phone: profile.phone || ''
        });
      }
    }
  }

  saveProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.userService.updateProfile(this.profileForm.value).subscribe({
      next: () => {
        this.userService.loadProfile().subscribe(); // Reload to sync state
        this.isLoading.set(false);
        this.isEditing.set(false);
        this.successMessage.set('Profile updated successfully!');
        setTimeout(() => this.successMessage.set(null), 3000);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to update profile.');
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage.set('File size exceeds 5MB limit.');
        return;
      }
      this.isLoading.set(true);
      this.userService.uploadProfilePicture(file).subscribe({
        next: () => {
          this.userService.loadProfile().subscribe();
          this.isLoading.set(false);
          this.successMessage.set('Profile picture updated!');
          setTimeout(() => this.successMessage.set(null), 3000);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.errorMessage.set(err.error?.message || 'Failed to upload picture.');
        }
      });
    }
  }

  removePicture() {
    this.isLoading.set(true);
    this.userService.removeProfilePicture().subscribe({
      next: () => {
        this.userService.loadProfile().subscribe();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('Failed to remove picture.');
      }
    });
  }

  getProfileImageUrl(fileName: string): string {
    return `http://localhost:5082/api/Users/profile/picture/${fileName}`;
  }
}
