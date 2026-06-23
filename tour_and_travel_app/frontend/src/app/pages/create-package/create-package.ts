import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { MetadataService, MetadataEnums } from '../../services/metadata.service';

@Component({
  selector: 'app-create-package',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-package.html',
  styleUrl: './create-package.css'
})
export class CreatePackageComponent {
  packageForm: FormGroup;
  mediaFiles: { file: File, preview: string, category: string, isPrimary: boolean, caption: string }[] = [];
  
  private fb = inject(FormBuilder);
  private packageService = inject(PackageService);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private metadataService = inject(MetadataService);
  
  isSubmitting = false;
  
  countries: string[] = [];
  metadataEnums: MetadataEnums | null = null;

  constructor() {
    this.packageForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      destination: ['', [Validators.required, Validators.maxLength(200)]],
      country: ['India', [Validators.required, Validators.maxLength(100)]],
      city: ['', [Validators.maxLength(100)]],
      durationDays: [1, [Validators.required, Validators.min(1), Validators.max(365)]],
      durationNights: [0, [Validators.min(0), Validators.max(365)]],
      maxCapacity: [10, [Validators.required, Validators.min(1), Validators.max(1000)]],
      minAge: [0, [Validators.min(0), Validators.max(120)]],
      cancellationPolicy: [''],
      type: ['Group', Validators.required],
      status: ['Draft', Validators.required],
      highlights: this.fb.array([]),
      inclusions: this.fb.array([]),
      seasonalPricing: this.fb.array([]),
      itinerary: this.fb.array([])
    });

    // Add initial default items
    this.addHighlight();
    this.addInclusion();
    this.addSeasonalPricing();
    this.addItineraryDay();
    
    this.loadMetadata();
  }
  
  loadMetadata() {
    this.metadataService.getCountries().subscribe({
      next: (res) => this.countries = res
    });
    this.metadataService.getEnums().subscribe({
      next: (res) => this.metadataEnums = res
    });
  }

  // Highlights
  get highlights() {
    return this.packageForm.get('highlights') as FormArray;
  }
  
  addHighlight() {
    this.highlights.push(this.fb.group({
      highlightText: ['', Validators.required],
      displayOrder: [this.highlights.length + 1, Validators.required]
    }));
  }
  
  removeHighlight(index: number) {
    this.highlights.removeAt(index);
  }

  // Inclusions
  get inclusions() {
    return this.packageForm.get('inclusions') as FormArray;
  }
  
  addInclusion() {
    this.inclusions.push(this.fb.group({
      description: ['', Validators.required],
      displayOrder: [this.inclusions.length + 1, Validators.required],
      inclusionType: ['included'] // included, excluded, optional
    }));
  }
  
  removeInclusion(index: number) {
    this.inclusions.removeAt(index);
  }

  // Seasonal Pricing
  get seasonalPricing() {
    return this.packageForm.get('seasonalPricing') as FormArray;
  }
  
  addSeasonalPricing() {
    this.seasonalPricing.push(this.fb.group({
      seasonName: ['', Validators.required],
      startDate: [null],
      endDate: [null],
      basePrice: [0, [Validators.required, Validators.min(0.01)]],
      childPrice: [0, Validators.min(0)],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      availableSlots: [10, [Validators.required, Validators.min(1)]],
      isActive: [true]
    }));
  }

  removeSeasonalPricing(index: number) {
    this.seasonalPricing.removeAt(index);
  }

  // Itinerary
  get itinerary() {
    return this.packageForm.get('itinerary') as FormArray;
  }

  addItineraryDay() {
    const dayIndex = this.itinerary.length + 1;
    this.itinerary.push(this.fb.group({
      dayNumber: [dayIndex, Validators.required],
      title: ['', Validators.required],
      description: [''],
      location: [''],
      activities: this.fb.array([]),
      meals: this.fb.array([]),
      accommodations: this.fb.array([]),
      transports: this.fb.array([])
    }));
  }

  removeItineraryDay(index: number) {
    this.itinerary.removeAt(index);
  }

  getActivities(dayIndex: number) {
    return this.itinerary.at(dayIndex).get('activities') as FormArray;
  }

  addActivity(dayIndex: number) {
    const activities = this.getActivities(dayIndex);
    activities.push(this.fb.group({
      sequenceOrder: [activities.length + 1, Validators.required],
      activityTitle: ['', Validators.required],
      description: [''],
      activityType: ['sightseeing'],
      location: [''],
      durationMinutes: [null],
      isOptional: [false],
      extraCost: [0],
      daySession: ['morning']
    }));
  }

  removeActivity(dayIndex: number, activityIndex: number) {
    this.getActivities(dayIndex).removeAt(activityIndex);
  }

  getMeals(dayIndex: number) {
    return this.itinerary.at(dayIndex).get('meals') as FormArray;
  }

  addMeal(dayIndex: number) {
    this.getMeals(dayIndex).push(this.fb.group({
      venue: [''],
      description: [''],
      isIncluded: [true],
      mealType: ['breakfast']
    }));
  }

  removeMeal(dayIndex: number, mealIndex: number) {
    this.getMeals(dayIndex).removeAt(mealIndex);
  }

  getAccommodations(dayIndex: number) {
    return this.itinerary.at(dayIndex).get('accommodations') as FormArray;
  }

  addAccommodation(dayIndex: number) {
    this.getAccommodations(dayIndex).push(this.fb.group({
      hotelName: ['', Validators.required],
      hotelAddress: [''],
      starRating: [null],
      roomType: [''],
      checkInTime: [''],
      checkOutTime: [''],
      amenities: [''],
      notes: ['']
    }));
  }

  removeAccommodation(dayIndex: number, accommodationIndex: number) {
    this.getAccommodations(dayIndex).removeAt(accommodationIndex);
  }

  getTransports(dayIndex: number) {
    return this.itinerary.at(dayIndex).get('transports') as FormArray;
  }

  addTransport(dayIndex: number) {
    const transports = this.getTransports(dayIndex);
    transports.push(this.fb.group({
      segmentOrder: [transports.length + 1, Validators.required],
      vehicleDescription: [''],
      pickupPoint: ['', Validators.required],
      dropPoint: ['', Validators.required],
      pickupTime: [''],
      dropTime: [''],
      distanceKm: [null],
      notes: [''],
      transportMode: ['bus']
    }));
  }

  removeTransport(dayIndex: number, transportIndex: number) {
    this.getTransports(dayIndex).removeAt(transportIndex);
  }

  // Media File Handling
  onFileSelected(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        if (file.type.match(/image\/*/) || file.type.match(/video\/*/)) {
          const reader = new FileReader();
          reader.onload = (e: any) => {
            this.mediaFiles.push({
              file: file,
              preview: e.target.result,
              category: 'gallery',
              isPrimary: this.mediaFiles.length === 0,
              caption: ''
            });
          };
          reader.readAsDataURL(file);
        } else {
          this.toastService.show('Only image and video files are allowed', 'error');
        }
      }
    }
  }

  removeMediaFile(index: number) {
    const wasPrimary = this.mediaFiles[index].isPrimary;
    this.mediaFiles.splice(index, 1);
    if (wasPrimary && this.mediaFiles.length > 0) {
      this.mediaFiles[0].isPrimary = true;
    }
  }

  setPrimaryMedia(index: number) {
    this.mediaFiles.forEach((m, i) => m.isPrimary = (i === index));
  }

  // Smooth Scrolling
  scrollToSection(sectionId: string) {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  // Submission
  onSubmit(status: 'Draft' | 'Active') {
    this.packageForm.patchValue({ status });

    if (this.packageForm.invalid && status === 'Active') {
      this.toastService.show('Please fill out all required fields correctly to publish.', 'error');
      // Mark all as touched to show validation errors
      this.packageForm.markAllAsTouched();
      return;
    }

    const formValue = this.packageForm.value;
    
    // Check validation rules from API
    const isFlexibleDate = ['Honeymoon', 'Private', 'Family'].includes(formValue.type);
    let hasValidationError = false;

    if (formValue.seasonalPricing && formValue.seasonalPricing.length > 0) {
      for (const pricing of formValue.seasonalPricing) {
        if (!isFlexibleDate && (!pricing.startDate || !pricing.endDate)) {
          this.toastService.show(`Start Date and End Date are required for ${formValue.type} packages in Seasonal Pricing.`, 'error');
          hasValidationError = true;
          break;
        }
      }
    }

    if (formValue.country && formValue.country.toLowerCase() !== 'india') {
      if (formValue.seasonalPricing && formValue.seasonalPricing.length > 0) {
        const validStartDates = formValue.seasonalPricing.map((p: any) => p.startDate).filter((d: any) => !!d);
        if (validStartDates.length > 0) {
          const earliestDate = new Date(Math.min(...validStartDates.map((d: string) => new Date(d).getTime())));
          const tenMonthsFromNow = new Date();
          tenMonthsFromNow.setMonth(tenMonthsFromNow.getMonth() + 10);
          
          if (earliestDate < tenMonthsFromNow) {
            this.toastService.show('International packages must be scheduled at least 10 months ahead.', 'error');
            hasValidationError = true;
          }
        }
      }

      if (!formValue.cancellationPolicy || !formValue.cancellationPolicy.toLowerCase().includes('3 months')) {
        this.toastService.show('International packages Cancellation Policy must mention "3 months" restriction.', 'error');
        hasValidationError = true;
      }
    }

    if (hasValidationError) return;

    this.isSubmitting = true;

    // Prepare CreatePackageRequest JSON
    const mediaMetadata = this.mediaFiles.map((m, idx) => ({
      fileName: m.file.name,
      caption: m.caption,
      displayOrder: idx + 1,
      isPrimary: m.isPrimary,
      category: m.category
    }));

    const packageData = {
      ...formValue,
      media: mediaMetadata
    };

    const formData = new FormData();
    formData.append('PackageData', JSON.stringify(packageData));
    
    this.mediaFiles.forEach(m => {
      formData.append('MediaFiles', m.file);
    });

    this.packageService.createPackage(formData).subscribe({
      next: (res) => {
        this.toastService.show(`Package ${status === 'Draft' ? 'saved as draft' : 'published'} successfully!`, 'success');
        this.router.navigate(['/packager/dashboard']);
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Failed to create package.', 'error');
        this.isSubmitting = false;
      }
    });
  }
}
