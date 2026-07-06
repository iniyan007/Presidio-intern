import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AgencyService } from '../../services/agency.service';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-apply-agency',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './apply-agency.html',
})
export class ApplyAgencyComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);
  private packagerService = inject(AgencyService);
  private router = inject(Router);
  private authService = inject(AuthService);

  applicationStatus = signal<any>(null);
  isLoadingStatus = signal(true);
  isAdmin = signal(false);

  currentStep = signal(1);
  isSubmitting = signal(false);
  errorMessage = signal('');

  applyForm: FormGroup = this.fb.group({
    companyName: ['', Validators.required],
    businessLicenseNo: ['', [Validators.required, Validators.minLength(5)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: ['', [Validators.required, Validators.pattern(/^\\d{10}$/)]],
    websiteUrl: ['', [Validators.required, Validators.pattern(/^(https?:\\/\\/)?([\\da-z.-]+)\\.([a-z.]{2,6})([/\\w .-]*)*\\/?$/)]],
    description: ['', [Validators.required, Validators.minLength(20)]]
  });

  panDocument: File | null = null;
  gstDocument: File | null = null;
  businessRegistration: File | null = null;

  ngOnInit() {
    if (this.authService.getUserRole() === 'Admin') {
      this.isAdmin.set(true);
      this.isLoadingStatus.set(false);
      return;
    }

    this.packagerService.getMyPackagerStatus().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.applicationStatus.set(res);
        this.isLoadingStatus.set(false);
      },
      error: () => {
        // No application found, show the form
        this.isLoadingStatus.set(false);
      }
    });
  }

  nextStep(step: number) {
    if (step === 2) {
      if (this.applyForm.get('companyName')?.invalid) {
        this.errorMessage.set('Company Name is required.');
        return;
      }
      if (this.applyForm.get('businessLicenseNo')?.invalid) {
        this.errorMessage.set('A valid Business License Number (min 5 characters) is required.');
        return;
      }
      if (!this.panDocument || !this.gstDocument || !this.businessRegistration) {
        this.errorMessage.set('Please upload all required documents.');
        return;
      }
    }
    
    if (step === 3) {
      if (this.applyForm.get('contactEmail')?.invalid) {
        this.errorMessage.set('A valid Email is required.');
        return;
      }
      if (this.applyForm.get('contactPhone')?.invalid) {
        this.errorMessage.set('A valid 10-digit Phone Number is required.');
        return;
      }
      if (this.applyForm.get('websiteUrl')?.invalid) {
        this.errorMessage.set('A valid Website URL is required.');
        return;
      }
      if (this.applyForm.get('description')?.invalid) {
        this.errorMessage.set('A valid Description (min 20 characters) is required.');
        return;
      }
    }
    
    this.errorMessage.set('');
    this.currentStep.set(step);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onFileSelected(event: Event, type: 'pan' | 'gst' | 'registration') {
    const element = event.target as HTMLInputElement;
    const file = element.files ? element.files[0] : null;
    if (file) {
      if (type === 'pan') this.panDocument = file;
      if (type === 'gst') this.gstDocument = file;
      if (type === 'registration') this.businessRegistration = file;
    }
  }

  submitApplication() {
    if (this.applyForm.invalid || !this.panDocument || !this.gstDocument || !this.businessRegistration) {
      this.errorMessage.set('Please fill out all required fields and upload documents.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const formData = new FormData();
    formData.append('CompanyName', this.applyForm.value.companyName);
    if (this.applyForm.value.businessLicenseNo) formData.append('BusinessLicenseNo', this.applyForm.value.businessLicenseNo);
    if (this.applyForm.value.contactEmail) formData.append('ContactEmail', this.applyForm.value.contactEmail);
    if (this.applyForm.value.contactPhone) formData.append('ContactPhone', this.applyForm.value.contactPhone);
    if (this.applyForm.value.websiteUrl) formData.append('WebsiteUrl', this.applyForm.value.websiteUrl);
    if (this.applyForm.value.description) formData.append('Description', this.applyForm.value.description);

    formData.append('PanDocument', this.panDocument);
    formData.append('GstDocument', this.gstDocument);
    formData.append('BusinessRegistration', this.businessRegistration);

    this.packagerService.applyToBecomePackager(formData).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to submit application.');
      }
    });
  }
}
