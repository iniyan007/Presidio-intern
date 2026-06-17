import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TravelPackage {
  id: string;
  packagerName: string;
  title: string;
  packageType: string;
  destination: string;
  country: string;
  durationDays: number;
  durationNights: number;
  avgRating: number;
  totalReviews: number;
  primaryImageUrl: string | null;
  startingPrice: number;
  pendingSeats: number;
  earliestDepartureDate: string | null;
  availableUntil: string | null;
}

export interface PackageMedia {
  id: string;
  filePath: string;
  caption: string | null;
  isPrimary: boolean;
  displayOrder: number;
}

export interface PackageSeasonalPricing {
  id: string;
  seasonName: string;
  startDate: string;
  endDate: string;
  basePrice: number;
  childPrice: number | null;
  discountPercent: number | null;
  availableSlots: number;
  isActive: boolean;
}

export interface ItineraryActivity {
  id: string;
  sequenceOrder: number;
  activityTitle: string;
  description: string | null;
  activityType: string | null;
  location: string | null;
  durationMinutes: number | null;
  isOptional: boolean;
  extraCost: number;
}

export interface ItineraryMeal {
  id: string;
  description: string | null;
  venue: string | null;
  isIncluded: boolean;
}

export interface ItineraryAccommodation {
  id: string;
  hotelName: string;
  hotelAddress: string | null;
  roomType: string | null;
  starRating: number | null;
  checkInTime: string | null;
  checkOutTime: string | null;
  amenities: string | null;
  notes: string | null;
}

export interface ItineraryTransport {
  id: string;
  segmentOrder: number;
  vehicleDescription: string;
  pickupPoint: string;
  dropPoint: string;
  pickupTime: string;
  dropTime: string;
  distanceKm: number;
  notes: string | null;
}

export interface ItineraryDay {
  id: string;
  dayNumber: number;
  title: string;
  description: string | null;
  location: string | null;
  activities: ItineraryActivity[];
  meals: ItineraryMeal[];
  accommodations: ItineraryAccommodation[];
  transports?: ItineraryTransport[];
}

export interface PackageReview {
  id: string;
  reviewerName: string;
  overallRating: number;
  comment: string | null;
  createdAt: string;
}

export interface TravelPackageDetails {
  id: string;
  packagerId: string;
  packagerName: string;
  title: string;
  packageType: string;
  description: string | null;
  destination: string;
  country: string;
  city: string | null;
  durationDays: number;
  durationNights: number;
  maxCapacity: number;
  currentBookings: number;
  minAge: number | null;
  cancellationPolicy: string | null;
  isFeatured: boolean;
  avgRating: number;
  totalReviews: number;
  highlights: string[];
  inclusions: string[];
  exclusions: string[];
  media: PackageMedia[];
  seasonalPricings: PackageSeasonalPricing[];
  itineraryDays: ItineraryDay[];
}

@Injectable({
  providedIn: 'root'
})
export class PackageService {
  private apiUrl = 'http://localhost:5082/api/Packages';
  private http = inject(HttpClient);

  constructor() { }

  getPackages(filters?: any): Observable<any> {
    let params = new HttpParams();
    if (filters) {
      if (filters.SearchTerm) params = params.set('SearchTerm', filters.SearchTerm);
      if (filters.Destination) params = params.set('Destination', filters.Destination);
      if (filters.MinPrice) params = params.set('MinPrice', filters.MinPrice);
      if (filters.MaxPrice) params = params.set('MaxPrice', filters.MaxPrice);
      if (filters.PackageType) params = params.set('PackageType', filters.PackageType);
      if (filters.TravelStartDate) params = params.set('TravelStartDate', filters.TravelStartDate);
      if (filters.SortBy) params = params.set('SortBy', filters.SortBy);
    }
    return this.http.get<any>(this.apiUrl, { params });
  }

  getPackageById(id: string): Observable<TravelPackageDetails> {
    return this.http.get<TravelPackageDetails>(`${this.apiUrl}/${id}`);
  }

  getPackageReviews(packageId: string): Observable<PackageReview[]> {
    return this.http.get<PackageReview[]>(`${this.apiUrl}/${packageId}/reviews`);
  }
}
