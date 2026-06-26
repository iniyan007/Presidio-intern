import { Component, inject, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { MetadataService, MetadataEnums } from '../../services/metadata.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-create-package',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-package.html',
  styleUrl: './create-package.css'
})
export class CreatePackageComponent implements OnInit, OnDestroy {
  packageForm: FormGroup;
  mediaFiles: { file: File, preview: string, category: string, isPrimary: boolean, caption: string }[] = [];
  
  private fb = inject(FormBuilder);
  private packageService = inject(PackageService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastService = inject(ToastService);
  private metadataService = inject(MetadataService);
  
  isSubmitting = false;
  private autoSaveSub?: Subscription;
  
  isEditMode = false;
  packageId: string | null = null;
  isPublished = false;
  existingPricings: any[] = [];
  existingItinerary: any[] = [];
  minDate = new Date().toISOString().split('T')[0];

  showPublishModal = false;
  publishUnderstandChecked = false;

  activeSection = 'basic-details';
  private sections = ['basic-details', 'logistics', 'highlights', 'media', 'pricing', 'itinerary'];
  
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
  }

  ngOnInit() {
    this.loadMetadata();
    // Set initial active section correctly if page starts scrolled down
    setTimeout(() => this.onWindowScroll(), 100);

    this.route.paramMap.subscribe(params => {
      this.packageId = params.get('id');
      const draftKey = 'tourmate_draft_' + (this.packageId || 'new');
      
      if (this.packageId) {
        this.isEditMode = true;
        const savedDraft = localStorage.getItem(draftKey);
        if (savedDraft && confirm('You have unsaved edits from a previous session. Would you like to restore them?')) {
           try {
             const draft = JSON.parse(savedDraft);
             // Need to fetch status to set isPublished
             this.packageService.getMyPackageById(this.packageId).subscribe(pkg => {
                this.isPublished = pkg.status === 'Published';
             });
             this.restoreDraft(draft);
           } catch(e) {
             this.loadPackageData(this.packageId);
           }
        } else {
          localStorage.removeItem(draftKey);
          this.loadPackageData(this.packageId);
        }
      } else {
        const savedDraft = localStorage.getItem(draftKey);
        if (savedDraft && confirm('You have unsaved changes from a previous session. Would you like to restore them?')) {
           try {
             const draft = JSON.parse(savedDraft);
             this.restoreDraft(draft);
           } catch(e) {
             localStorage.removeItem(draftKey);
           }
        } else {
           localStorage.removeItem(draftKey);
        }
      }

      this.autoSaveSub = this.packageForm.valueChanges.pipe(debounceTime(1000)).subscribe(val => {
        localStorage.setItem(draftKey, JSON.stringify(val));
      });
    });
  }

  ngOnDestroy() {
    if (this.autoSaveSub) {
      this.autoSaveSub.unsubscribe();
    }
  }

  restoreDraft(draft: any) {
    this.highlights.clear();
    draft.highlights?.forEach(() => this.addHighlight());
    
    this.inclusions.clear();
    draft.inclusions?.forEach(() => this.addInclusion());
    
    this.seasonalPricing.clear();
    draft.seasonalPricing?.forEach(() => this.addSeasonalPricing());

    this.itinerary.clear();
    draft.itinerary?.forEach((day: any, i: number) => {
      this.addItineraryDay();
      
      day.activities?.forEach(() => this.addActivity(i));
      day.meals?.forEach(() => this.addMeal(i));
      day.accommodations?.forEach(() => this.addAccommodation(i));
      day.transports?.forEach(() => this.addTransport(i));
    });

    this.packageForm.patchValue(draft);
  }
  
  @HostListener('window:scroll')
  onWindowScroll() {
    let currentSection = this.sections[0];
    const scrollPosition = window.scrollY + 120; // offset for detection

    for (const section of this.sections) {
      const element = document.getElementById(section);
      if (element) {
        const top = element.offsetTop;
        if (scrollPosition >= top) {
          currentSection = section;
        }
      }
    }
    
    // If user has scrolled to the absolute bottom of the page, activate the last section
    if ((window.innerHeight + Math.ceil(window.scrollY)) >= document.body.offsetHeight - 50) {
      currentSection = this.sections[this.sections.length - 1];
    }
    
    if (this.activeSection !== currentSection) {
      setTimeout(() => this.activeSection = currentSection);
    }
  }
  
  loadMetadata() {
    this.metadataService.getCountries().subscribe({
      next: (res) => setTimeout(() => this.countries = res)
    });
    this.metadataService.getEnums().subscribe({
      next: (res) => setTimeout(() => this.metadataEnums = res)
    });
  }

  loadPackageData(id: string) {
    this.packageService.getMyPackageById(id).subscribe({
      next: (pkg) => {
        // Clear default empty arrays
        this.highlights.clear();
        this.inclusions.clear();
        this.seasonalPricing.clear();
        this.itinerary.clear();

        this.isPublished = pkg.status === 'Published';
        // Basic details
        this.packageForm.patchValue({
          title: pkg.title,
          description: pkg.description,
          destination: pkg.destination,
          country: pkg.country,
          city: pkg.city,
          durationDays: pkg.durationDays,
          durationNights: pkg.durationNights,
          maxCapacity: pkg.maxCapacity,
          minAge: pkg.minAge,
          cancellationPolicy: pkg.cancellationPolicy,
          type: pkg.packageType,
          status: pkg.status
        });

        if (this.isPublished) {
          ['title', 'description', 'destination', 'country', 'city', 'durationDays', 'durationNights', 'maxCapacity', 'minAge', 'cancellationPolicy', 'type'].forEach(field => {
            this.packageForm.get(field)?.disable();
          });
        }

        // Highlights
        this.highlights.clear();
        if (pkg.highlights) {
          pkg.highlights.filter(h => h && h.trim() !== '').forEach(h => {
            this.highlights.push(this.fb.group({
              highlightText: [h, Validators.required]
            }));
          });
        }

        // Inclusions
        this.inclusions.clear();
        let inclusionCounter = 1;
        if (pkg.inclusions) {
          pkg.inclusions.filter(inc => inc && inc.trim() !== '').forEach((inc) => {
            this.inclusions.push(this.fb.group({
              description: [inc, Validators.required],
              displayOrder: [inclusionCounter++, Validators.required],
              inclusionType: ['Included']
            }));
          });
        }
        
        // Exclusions
        if (pkg.exclusions) {
          pkg.exclusions.filter(exc => exc && exc.trim() !== '').forEach((exc) => {
            this.inclusions.push(this.fb.group({
              description: [exc, Validators.required],
              displayOrder: [inclusionCounter++, Validators.required],
              inclusionType: ['Excluded']
            }));
          });
        }

        
          // Seasonal Pricing
          this.seasonalPricing.clear();
          if (this.isPublished) {
            this.existingPricings = pkg.seasonalPricings || [];
          } else {
            pkg.seasonalPricings?.forEach(sp => {
              this.seasonalPricing.push(this.fb.group({
                seasonName: [sp.seasonName, Validators.required],
                startDate: [sp.startDate],
                endDate: [sp.endDate],
                basePrice: [sp.basePrice, [Validators.required, Validators.min(0.01)]],
                childPrice: [sp.childPrice, Validators.min(0)],
                discountPercent: [sp.discountPercent, [Validators.min(0), Validators.max(100)]],
                availableSlots: [sp.availableSlots, [Validators.required, Validators.min(1)]],
                isActive: [sp.isActive]
              }));
            });
          }

          // Itinerary
          this.itinerary.clear();
          if (this.isPublished) {
            this.existingItinerary = pkg.itineraryDays || [];
          } else {
            pkg.itineraryDays?.forEach(day => {

          const dayGroup = this.fb.group({
            dayNumber: [day.dayNumber, Validators.required],
            title: [day.title, Validators.required],
            description: [day.description],
            location: [day.location],
            activities: this.fb.array([]),
            meals: this.fb.array([]),
            accommodations: this.fb.array([]),
            transports: this.fb.array([])
          });

          const activitiesArray = dayGroup.get('activities') as FormArray;
          day.activities?.forEach((a: any) => {
            activitiesArray.push(this.fb.group({
              sequenceOrder: [a.sequenceOrder, Validators.required],
              activityTitle: [a.activityTitle, Validators.required],
              description: [a.description],
              activityType: [a.activityType, Validators.required],
              location: [a.location],
              durationMinutes: [a.durationMinutes],
              isOptional: [a.isOptional],
              extraCost: [a.extraCost],
              daySession: [a.daySession?.toLowerCase() || 'morning']
            }));
          });

          const mealsArray = dayGroup.get('meals') as FormArray;
          day.meals?.forEach((m: any) => {
            mealsArray.push(this.fb.group({
              venue: [m.venue],
              description: [m.description],
              isIncluded: [m.isIncluded],
              mealType: [m.mealType?.toLowerCase() || 'breakfast']
            }));
          });

          const accommodationsArray = dayGroup.get('accommodations') as FormArray;
          day.accommodations?.forEach((acc: any) => {
            accommodationsArray.push(this.fb.group({
              hotelName: [acc.hotelName, Validators.required],
              hotelAddress: [acc.hotelAddress],
              starRating: [acc.starRating],
              roomType: [acc.roomType],
              checkInTime: [acc.checkInTime],
              checkOutTime: [acc.checkOutTime],
              amenities: [acc.amenities],
              notes: [acc.notes]
            }));
          });

          const transportsArray = dayGroup.get('transports') as FormArray;
          day.transports?.forEach((tr: any) => {
            transportsArray.push(this.fb.group({
              segmentOrder: [tr.segmentOrder, Validators.required],
              vehicleDescription: [tr.vehicleDescription],
              pickupPoint: [tr.pickupPoint, Validators.required],
              dropPoint: [tr.dropPoint, Validators.required],
              pickupTime: [tr.pickupTime],
              dropTime: [tr.dropTime],
              distanceKm: [tr.distanceKm],
              notes: [tr.notes],
              transportMode: [tr.transportMode?.toLowerCase() || 'bus']
            }));
          });

          this.itinerary.push(dayGroup);
        });
        }

        // Media
        pkg.media?.forEach(m => {
           const file = new File([""], m.fileName, { type: m.mimeType || "image/jpeg" });
           this.mediaFiles.push({
             file: file,
             preview: m.filePath.startsWith('http') ? m.filePath : `${environment.baseUrl}${m.filePath}`,
             category: m.category || 'Destination',
             isPrimary: m.isPrimary,
             caption: m.caption || ''
           });
        });
        
        // Ensure at least one empty item if arrays are empty
        if (this.highlights.length === 0) this.addHighlight();
        if (this.inclusions.length === 0) this.addInclusion();
        if (this.seasonalPricing.length === 0) this.addSeasonalPricing();
        if (this.itinerary.length === 0 && !this.isPublished) this.addItineraryDay();
      },
      error: (err) => {
        this.toastService.show('Failed to load package data.', 'error');
      }
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
      inclusionType: ['Included'] // Included, Excluded, Optional
    }));
  }
  
  removeInclusion(index: number) {
    this.inclusions.removeAt(index);
  }

  // Seasonal Pricing
  get seasonalPricing() {
    return this.packageForm.get('seasonalPricing') as FormArray;
  }
  
  dateRangeValidator(group: FormGroup) {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (start && end && new Date(end) < new Date(start)) {
      return { dateRange: true };
    }
    return null;
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
    }, { validators: this.dateRangeValidator }));
  }

  removeSeasonalPricing(index: number) {
    this.seasonalPricing.removeAt(index);
  }

  getMinEndDate(spIndex: number): string {
    const startDate = this.seasonalPricing.at(spIndex).get('startDate')?.value;
    return startDate ? startDate : this.minDate;
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
      activityType: ['', Validators.required],
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
    if (files) {
      if (this.mediaFiles.length + files.length > 10) {
        this.toastService.show('You can upload a maximum of 10 images.', 'error');
        event.target.value = ''; // Reset input
        return;
      }
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        if (file.type.match(/image\/*/) || file.type.match(/video\/*/)) {
          const reader = new FileReader();
          reader.onload = (e: any) => {
            this.mediaFiles.push({
              file: file,
              preview: e.target.result,
              category: 'Destination',
              isPrimary: this.mediaFiles.length === 0,
              caption: ''
            });
          };
          reader.readAsDataURL(file);
        } else {
          this.toastService.show('Unsupported file type', 'error');
        }
      }
      event.target.value = ''; // Reset input to allow selecting same files again if removed
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
      const y = element.getBoundingClientRect().top + window.scrollY - 100; // 100px offset for navbar
      window.scrollTo({ top: y, behavior: 'smooth' });
    }
  }

    // Publish Modal
  openPublishModal() {
    if (this.packageForm.invalid) {
      this.toastService.show('Please fill out all required fields correctly to publish.', 'error');
      this.packageForm.markAllAsTouched();
      return;
    }
    
    // Check validation rules from API for frontend pre-validation
    const formValue = this.packageForm.getRawValue();
    const isFlexibleDate = ['Honeymoon', 'Private', 'Family'].includes(formValue.type);
    let hasValidationError = false;

    if (!formValue.seasonalPricing || formValue.seasonalPricing.length === 0) {
      this.toastService.show('You must add at least one Pricing Season to publish a package.', 'error');
      hasValidationError = true;
    } else {
      for (const pricing of formValue.seasonalPricing) {
        if (!isFlexibleDate && (!pricing.startDate || !pricing.endDate)) {
          this.toastService.show(`Start Date and End Date are required for ${formValue.type} packages in Seasonal Pricing.`, 'error');
          hasValidationError = true;
          break;
        }
        if (pricing.startDate && pricing.endDate && new Date(pricing.endDate) < new Date(pricing.startDate)) {
          this.toastService.show('End Date cannot be earlier than Start Date in Seasonal Pricing.', 'error');
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


    if (formValue.durationDays !== formValue.itinerary.length) {
      this.toastService.show(`The detailed itinerary must have exactly ${formValue.durationDays} days to match the package duration. Currently it has ${formValue.itinerary.length} days.`, 'error');
      hasValidationError = true;
    }

    if (!hasValidationError) {
      this.showPublishModal = true;
      this.publishUnderstandChecked = false;
    }
  }

  closePublishModal() {
    this.showPublishModal = false;
  }

  confirmPublish() {
    if (this.publishUnderstandChecked) {
      this.closePublishModal();
      this.onSubmit('Published');
    }
  }

  // Submission
  onSubmit(status: 'Draft' | 'Published') {
    if (this.isSubmitting) return;
    this.packageForm.patchValue({ status });

    const formValue = this.packageForm.getRawValue();
    this.isSubmitting = true;

    // Prepare CreatePackageRequest JSON
    const mediaMetadata = this.mediaFiles.map((m, idx) => ({
      fileName: m.file.name,
      caption: m.caption,
      displayOrder: idx + 1,
      isPrimary: m.isPrimary,
      category: m.category
    }));

    const isFlexibleDateType = ['Honeymoon', 'Private', 'Family'].includes(formValue.type);

    const packageData = {
      ...formValue,
      seasonalPricing: formValue.seasonalPricing.map((p: any) => ({
        id: p.id || null,
        seasonName: p.seasonName,
        startDate: p.startDate,
        endDate: p.endDate,
        basePrice: p.basePrice,
        childPrice: p.childPrice,
        discountPercent: p.discountPercent,
        availableSlots: p.availableSlots,
        isActive: p.isActive
      })),
      itinerary: isFlexibleDateType ? [] : formValue.itinerary,
      media: mediaMetadata
    };
    
    // Filter out any empty highlights/inclusions before saving
    packageData.highlights = packageData.highlights.filter((h: any) => h.highlightText && h.highlightText.trim() !== '');
    packageData.inclusions = packageData.inclusions.filter((i: any) => i.description && i.description.trim() !== '');

    if (status === 'Draft') {
      if (!packageData.title || !packageData.title.trim()) {
        this.toastService.show('Please provide a Package Title to save a draft.', 'error');
        this.isSubmitting = false;
        return;
      }
      if (!packageData.destination || !packageData.destination.trim()) {
        this.toastService.show('Please provide a Destination Region to save a draft.', 'error');
        this.isSubmitting = false;
        return;
      }

      // Filter out empty items that would fail backend validation
      packageData.highlights = packageData.highlights.filter((h: any) => h.highlightText && h.highlightText.trim() !== '');
      packageData.inclusions = packageData.inclusions.filter((i: any) => i.description && i.description.trim() !== '');
      // Assign default values to empty items so they can be saved as draft
      packageData.seasonalPricing.forEach((p: any) => {
        if (!p.seasonName || p.seasonName.trim() === '') p.seasonName = 'Draft Season';
        if (p.basePrice == null) p.basePrice = 0;
        if (p.availableSlots == null) p.availableSlots = 10;
      });
      
      packageData.itinerary = packageData.itinerary.filter((day: any) => day.title && day.title.trim() !== '');
      packageData.itinerary.forEach((day: any) => {
        day.activities = day.activities.filter((a: any) => a.activityTitle && a.activityTitle.trim() !== '');
        day.accommodations = day.accommodations.filter((a: any) => a.hotelName && a.hotelName.trim() !== '');
        day.transports = day.transports.filter((t: any) => t.pickupPoint && t.pickupPoint.trim() !== '' && t.dropPoint && t.dropPoint.trim() !== '');
        day.meals = day.meals.filter((m: any) => (m.description && m.description.trim() !== '') || (m.venue && m.venue.trim() !== ''));
      });
    }

    const formData = new FormData();
    formData.append('PackageData', JSON.stringify(packageData));
    
    this.mediaFiles.forEach(m => {
      formData.append('MediaFiles', m.file);
    });

    if (this.isEditMode && this.packageId) {
      this.packageService.updateFullPackage(this.packageId, formData).subscribe({
        next: (res) => {
          this.toastService.show(`Package ${status === 'Draft' ? 'draft updated' : 'published'} successfully!`, 'success');
          this.router.navigate(['/packager/dashboard']);
        },
        error: (err) => {
          this.toastService.show(err.error?.message || 'Failed to update package.', 'error');
          this.isSubmitting = false;
        }
      });
    } else {
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
}
