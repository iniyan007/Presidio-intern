export interface TravelPackage {
  id: string;
  packagerName: string;
  title: string;
  packageType: string;
  status: string;
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
  maxCapacity: number;
}

export interface PackageMedia {
  id: string;
  fileName: string;
  filePath: string;
  mimeType: string | null;
  category: string;
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
  daySession?: string;
}

export interface ItineraryMeal {
  id: string;
  description: string | null;
  venue: string | null;
  isIncluded: boolean;
  mealType?: string;
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
  distanceKm: number | null;
  notes: string | null;
  transportMode?: string;
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
  status: string;
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
  avgAccommodationRating?: number;
  avgTransportRating?: number;
  avgFoodRating?: number;
  avgGuideRating?: number;
  avgValueRating?: number;
  highlights: string[];
  inclusions: string[];
  exclusions: string[];
  media: PackageMedia[];
  seasonalPricings: PackageSeasonalPricing[];
  itineraryDays: ItineraryDay[];
}

export interface WishlistResponse {
  wishlistId: string;
  packageId: string;
  addedAt: string;
  package: TravelPackage;
}
