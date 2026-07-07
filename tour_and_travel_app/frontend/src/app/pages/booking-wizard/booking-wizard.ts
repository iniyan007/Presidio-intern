import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, OnInit, computed, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PackageService } from '../../services/package.service';
import { TravelPackageDetails, PackageSeasonalPricing } from '../../models/package.model';
import { BookingService } from '../../services/booking.service';
import { PlatformConfigResponse } from '../../models/booking.model';
import { ToastService } from '../../services/toast.service';

interface TravelerForm {
  fullName: string;
  dateOfBirth: string;
  gender: string;
  nationality: string;
  passportNumber: string;
  aadharCardNumber: string;
  mealPreference: string;
  isPrimary: boolean;
  passportFile: File | null;
  aadharFile: File | null;
}

@Component({
  selector: 'app-booking-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './booking-wizard.html',
  styleUrls: ['./booking-wizard.css']
})
export class BookingWizardComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private packageService = inject(PackageService);
  private bookingService = inject(BookingService);
  private toastService = inject(ToastService);

  step = signal<number>(1);
  pkg = signal<TravelPackageDetails | null>(null);
  platformConfig = signal<PlatformConfigResponse | null>(null);

  // Form State
  travelDate = signal<string>('');
  infantCount = signal<number>(0);
  specialRequests = signal<string>('');

  travelers = signal<TravelerForm[]>([
    {
      fullName: '', dateOfBirth: '', gender: 'Male', nationality: 'Indian',
      passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian',
      isPrimary: true, passportFile: null, aadharFile: null
    }
  ]);

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  selectedSeason = signal<PackageSeasonalPricing | null>(null);
  isFixedDate = signal<boolean>(false);

  seasonStart = computed(() => this.selectedSeason()?.startDate.split('T')[0]);
  seasonEnd = computed(() => this.selectedSeason()?.endDate.split('T')[0]);

  maxAllowedTravelers = computed(() => {
    const season = this.selectedSeason();
    if (!season) return 0;
    
    const pkgType = this.pkg()?.packageType;
    if (pkgType === 'Honeymoon') {
      return 2;
    } else if (pkgType === 'Family') {
      return 10;
    } else if (pkgType === 'Private') {
      return 4;
    }
    
    return season.availableSlots;
  });

  adultCount = computed(() => {
    return this.travelers().filter(t => {
      if (!t.dateOfBirth) return true; // Assume adult if no DOB
      const age = this.calculateAge(new Date(t.dateOfBirth));
      return age >= 12;
    }).length;
  });

  childCount = computed(() => {
    return this.travelers().filter(t => {
      if (!t.dateOfBirth) return false;
      const age = this.calculateAge(new Date(t.dateOfBirth));
      return age >= 2 && age < 12;
    }).length;
  });

  pricing = computed(() => {
    const season = this.selectedSeason();
    const config = this.platformConfig();

    if (!season || !config) return { baseTotal: 0, discount: 0, platformFee: 0, gst: 0, grandTotal: 0 };

    const baseTotal = (this.adultCount() * season.basePrice) + (this.childCount() * (season.childPrice || season.basePrice));
    const discount = season.discountPercent ? (baseTotal * season.discountPercent / 100) : 0;
    const afterDiscount = baseTotal - discount;
    const platformFee = afterDiscount * (config.platformFeePercent / 100);
    const gst = (afterDiscount + platformFee) * (config.gstPercent / 100);
    const grandTotal = afterDiscount + platformFee + gst;

    return { baseTotal, discount, platformFee, gst, grandTotal };
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    const seasonIdParam = this.route.snapshot.queryParamMap.get('seasonId');
    if (id) {
      this.packageService.getPackageById(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (p: TravelPackageDetails) => {
          this.pkg.set(p);

          let loadedDateFromDraft = false;
          let loadedTravelersFromDraft = false;
          const draftStr = localStorage.getItem(`bookingDraft_${id}`);
          if (draftStr) {
            try {
              const draft = JSON.parse(draftStr);
              if (draft.travelDate) {
                this.travelDate.set(draft.travelDate);
                loadedDateFromDraft = true;
              }
              if (draft.infantCount !== undefined) this.infantCount.set(draft.infantCount);
              if (draft.specialRequests) this.specialRequests.set(draft.specialRequests);
              if (draft.travelers && Array.isArray(draft.travelers) && draft.travelers.length > 0) {
                this.travelers.set(draft.travelers);
                loadedTravelersFromDraft = true;
              }
            } catch (e) { }
          }

          if (!loadedTravelersFromDraft) {
            if (p.packageType === 'Honeymoon') {
              if (this.travelers().length === 1) {
                this.travelers.update(t => [...t, {
                  fullName: '', dateOfBirth: '', gender: 'Female', nationality: 'Indian',
                  passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian',
                  isPrimary: false, passportFile: null, aadharFile: null
                }]);
              }
            } else if (p.packageType === 'Family') {
              while (this.travelers().length < 3) {
                this.travelers.update(t => [...t, {
                  fullName: '', dateOfBirth: '', gender: 'Male', nationality: 'Indian',
                  passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian',
                  isPrimary: false, passportFile: null, aadharFile: null
                }]);
              }
            }
          }

          if (seasonIdParam) {
            const season = p.seasonalPricings.find(s => s.id === seasonIdParam);
            if (season) {
              this.selectedSeason.set(season);
              if (p.packageType === 'Group' || p.packageType === 'Pilgrimage' || p.packageType === 'Adventure') {
                this.isFixedDate.set(true);
                this.travelDate.set(season.startDate.split('T')[0]); // Ensure YYYY-MM-DD
              } else {
                this.isFixedDate.set(false);
                if (!loadedDateFromDraft) {
                  this.travelDate.set(season.startDate.split('T')[0]);
                }
              }
            }
          } else if (!loadedDateFromDraft) {
            const today = new Date().toISOString().split('T')[0];
            this.travelDate.set(today);
          }
        },
        error: () => this.router.navigate(['/'])
      });

      this.bookingService.getPlatformConfig().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (config) => this.platformConfig.set(config),
        error: (err) => console.error('Failed to load platform config', err)
      });
    }
  }

  calculateAge(birthday: Date): number {
    const ageDifMs = Date.now() - birthday.getTime();
    const ageDate = new Date(ageDifMs);
    return Math.abs(ageDate.getUTCFullYear() - 1970);
  }

  addTraveler() {
    const maxSlots = this.maxAllowedTravelers();
    if (this.travelers().length >= maxSlots) {
      this.toastService.show(`Cannot add more travelers. Only ${maxSlots} slots are allowed for this package/season.`, 'error');
      return;
    }

    this.travelers.update(t => [...t, {
      fullName: '', dateOfBirth: '', gender: 'Male', nationality: 'Indian',
      passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian',
      isPrimary: false, passportFile: null, aadharFile: null
    }]);
  }

  removeTraveler(index: number) {
    if (index === 0) return; // Cannot remove primary
    const pkgType = this.pkg()?.packageType;
    const currentLength = this.travelers().length;
    
    if (pkgType === 'Honeymoon' && currentLength <= 2) {
      this.toastService.show('Honeymoon packages require exactly 2 travelers.', 'error');
      return;
    }
    if (pkgType === 'Family' && currentLength <= 3) {
      this.toastService.show('Family packages require a minimum of 3 travelers.', 'error');
      return;
    }
    
    this.travelers.update(t => t.filter((_, i) => i !== index));
  }

  getDraftKey(): string | null {
    const id = this.pkg()?.id;
    return id ? `bookingDraft_${id}` : null;
  }

  saveDraft() {
    const key = this.getDraftKey();
    if (!key) return;

    const travelersData = this.travelers().map(t => ({
      ...t,
      passportFile: null,
      aadharFile: null
    }));

    const draft = {
      travelDate: this.travelDate(),
      infantCount: this.infantCount(),
      specialRequests: this.specialRequests(),
      travelers: travelersData
    };

    localStorage.setItem(key, JSON.stringify(draft));
    this.toastService.show('Draft saved successfully! You can safely leave and resume later.', 'success');
  }

  clearDraft() {
    const key = this.getDraftKey();
    if (key) localStorage.removeItem(key);
  }

  updateTraveler(index: number, field: keyof TravelerForm, value: any) {
    this.travelers.update(ts => {
      const updated = [...ts];
      updated[index] = { ...updated[index], [field]: value };
      return updated;
    });
  }

  setStep(s: number) {
    // Basic validation before moving forward
    if (s > this.step()) {
      if (this.step() === 1) {
        if (!this.selectedSeason()) {
          this.toastService.show('No active season is selected.', 'error');
          return;
        }

        const pkgType = this.pkg()?.packageType;
        const totalTravelers = this.travelers().length;

        if (pkgType === 'Honeymoon') {
          if (totalTravelers !== 2) {
            this.toastService.show('Honeymoon packages require exactly 2 travelers per booking.', 'error');
            return;
          }
          this.infantCount.set(0);
        } else if (pkgType === 'Family') {
          if (totalTravelers < 3 || totalTravelers > 10) {
            this.toastService.show('Family packages require 3 to 10 travelers per booking.', 'error');
            return;
          }
        } else if (pkgType === 'Private') {
          if (totalTravelers < 1 || totalTravelers > 4) {
            this.toastService.show('Private packages require 1 to 4 travelers per booking.', 'error');
            return;
          }
        }

        if (this.infantCount() > 10) {
          this.toastService.show('Maximum 10 infants are allowed per booking.', 'error');
          return;
        }

        const season = this.selectedSeason()!;
        const selectedDateStr = this.travelDate();
        const startDateStr = season.startDate.split('T')[0];
        const endDateStr = season.endDate.split('T')[0];

        if (selectedDateStr < startDateStr || selectedDateStr > endDateStr) {
          this.toastService.show('Travel Date must be within the selected season range: ' + startDateStr + ' to ' + endDateStr, 'error');
          return;
        }

        const isIndia = this.pkg()?.country?.trim().toLowerCase() === 'india';
        let hasValidationErrors = false;

        for (let i = 0; i < this.travelers().length; i++) {
          const t = this.travelers()[i];

          if (!t.fullName || !t.dateOfBirth || !t.aadharCardNumber) {
            this.toastService.show(`Traveler ${i + 1}: Please fill out all mandatory fields (Name, DOB, Aadhar).`, 'error');
            hasValidationErrors = true;
            break;
          }

          const nameRegex = /^[a-zA-Z\s]+$/;
          if (!nameRegex.test(t.fullName.trim())) {
            this.toastService.show(`Traveler ${i + 1}: Name can only contain alphabets and spaces.`, 'error');
            hasValidationErrors = true;
            break;
          }

          const dobDate = new Date(t.dateOfBirth);
          if (dobDate > new Date()) {
            this.toastService.show(`Traveler ${i + 1}: Date of Birth cannot be in the future.`, 'error');
            hasValidationErrors = true;
            break;
          }

          const age = this.calculateAge(dobDate);
          if (t.isPrimary && age < 18) {
            this.toastService.show(`Traveler ${i + 1} (Primary): Must be at least 18 years old to book.`, 'error');
            hasValidationErrors = true;
            break;
          }

          if (this.pkg()?.packageType === 'Honeymoon' && age < 18) {
            this.toastService.show(`Traveler ${i + 1} (${t.fullName}): All travelers must be at least 18 years old for Honeymoon packages.`, 'error');
            hasValidationErrors = true;
            break;
          }

          const aadharRegex = /^\d{12}$/;
          if (!aadharRegex.test(t.aadharCardNumber.replace(/\s/g, ''))) {
            this.toastService.show(`Traveler ${i + 1} (${t.fullName}): Aadhar Number must be exactly 12 digits.`, 'error');
            hasValidationErrors = true;
            break;
          }

          if (!isIndia && !t.passportNumber) {
            this.toastService.show(`Traveler ${i + 1} (${t.fullName}): Passport Number is COMPULSORY for international packages.`, 'error');
            hasValidationErrors = true;
            break;
          }

          if (t.passportNumber) {
            const passportRegex = /^[A-Z0-9]{6,9}$/i;
            if (!passportRegex.test(t.passportNumber.trim())) {
              this.toastService.show(`Traveler ${i + 1} (${t.fullName}): Invalid Passport Number format.`, 'error');
              hasValidationErrors = true;
              break;
            }
          }
        }

        if (hasValidationErrors) return;
      }
      if (this.step() === 2) {
        // Require Aadhar for all
        const missingDocs = this.travelers().find(t => !t.aadharFile);
        if (missingDocs) {
          this.toastService.show('Please upload Aadhar card for all travelers.', 'error');
          return;
        }

        const isIndia = this.pkg()?.country?.trim().toLowerCase() === 'india';
        if (!isIndia) {
          const missingPassport = this.travelers().find(t => !t.passportFile);
          if (missingPassport) {
            this.toastService.show('Passport scan is mandatory for all travelers on international packages.', 'error');
            return;
          }
        }
      }
    }
    this.step.set(s);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onFileSelected(event: any, travelerIndex: number, type: 'passport' | 'aadhar') {
    const file = event.target.files[0];
    if (file) {
      this.travelers.update(ts => {
        const updated = [...ts];
        if (type === 'passport') updated[travelerIndex].passportFile = file;
        if (type === 'aadhar') updated[travelerIndex].aadharFile = file;
        return updated;
      });
    }
  }

  removeFile(travelerIndex: number, type: 'passport' | 'aadhar') {
    this.travelers.update(ts => {
      const updated = [...ts];
      if (type === 'passport') updated[travelerIndex].passportFile = null;
      if (type === 'aadhar') updated[travelerIndex].aadharFile = null;
      return updated;
    });
  }

  submitBooking() {
    const p = this.pkg();
    const season = this.selectedSeason();
    if (!p || !season) return;

    this.isSubmitting.set(true);

    const bookingData = {
      packageId: p.id,
      seasonalPricingId: season.id,
      travelDate: this.travelDate(),
      infantCount: this.infantCount(),
      specialRequests: this.specialRequests(),
      travelers: this.travelers().map(t => ({
        fullName: t.fullName,
        dateOfBirth: t.dateOfBirth ? t.dateOfBirth : null,
        gender: t.gender,
        nationality: t.nationality,
        passportNumber: t.passportNumber,
        aadharCardNumber: t.aadharCardNumber,
        mealPreference: t.mealPreference,
        isPrimary: t.isPrimary,
        aadharCardFileName: t.aadharFile ? t.aadharFile.name : null,
        passportFileName: t.passportFile ? t.passportFile.name : null
      }))
    };

    const formData = new FormData();
    formData.append('bookingData', JSON.stringify(bookingData));

    // Append all actual files
    this.travelers().forEach(t => {
      if (t.aadharFile) formData.append('documentFiles', t.aadharFile);
      if (t.passportFile) formData.append('documentFiles', t.passportFile);
    });

    this.bookingService.createBooking(formData).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.clearDraft();
        this.isSubmitting.set(false);
        this.router.navigate(['/payment', res.id]);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set(err.error?.message || err.message || 'An error occurred while creating booking.');
        this.isSubmitting.set(false);
      }
    });
  }
}
