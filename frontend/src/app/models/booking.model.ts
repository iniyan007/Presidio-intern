import { Trip } from './trip.model';
import { User } from './user.model';

export interface BookingSeat {
  id: number;
  bookingId: number;
  seatNumber: number;
  passengerName: string;
  passengerAge: number;
  passengerGender: string;
}

export interface Booking {
  id: number;
  tripId: number;
  userId: number;
  totalAmount: number;
  status: string;
  lockedUntil: string;
  createdAt: string;
  trip?: Trip;
  user?: User;
  bookingSeats?: BookingSeat[];
}
