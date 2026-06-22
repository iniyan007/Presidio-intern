export interface TravelDocumentResponse {
  id: string;
  documentType: string;
  filePath: string;
  fileName: string;
  uploadedAt: string;
  status: string;
  rejectionReason: string | null;
}

export interface BookingTravelerResponse {
  id: string;
  fullName: string;
  passportNumber: string | null;
  dateOfBirth: string | null;
  nationality: string | null;
  age: number | null;
  gender: string | null;
  mealPreference: string | null;
  aadharCardNumber: string | null;
  isPrimary: boolean;
  documents: TravelDocumentResponse[];
}

export interface BookingResponse {
  id: string;
  userId: string;
  packageId: string;
  bookingReference: string;
  adultCount: number;
  childCount: number;
  infantCount: number;
  totalAmount: number;
  paidAmount: number;
  status: string;
  paymentStatus: string;
  travelDate: string;
  returnDate: string;
  specialRequests: string | null;
  bookedAt: string;
  cancelledAt: string | null;
  cancellationReason: string | null;
  travelers: BookingTravelerResponse[];
}

export interface CancelBookingRequest {
  reason: string;
}

export interface ProcessPaymentRequest {
  amount: number;
  paymentMethod: string;
  transactionId: string;
}

export interface PlatformConfigResponse {
  id: string;
  platformFeePercent: number;
  gstPercent: number;
  note: string | null;
  updatedBy: string | null;
  updatedAt: string;
}

export interface VerifyDocumentRequest {
  isVerified: boolean;
  rejectionReason: string | null;
}
