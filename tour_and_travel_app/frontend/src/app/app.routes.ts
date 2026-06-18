import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { PackageDetailsComponent } from './pages/package-details/package-details';
import { ProfileComponent } from './pages/profile/profile';
import { MyBookingsComponent } from './pages/my-bookings/my-bookings';
import { BookingWizardComponent } from './pages/booking-wizard/booking-wizard';
import { PaymentComponent } from './pages/payment/payment';
import { WishlistComponent } from './pages/wishlist/wishlist';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'auth', component: AuthComponent },
  { path: 'package/:id', component: PackageDetailsComponent },
  { path: 'package/:id/book', component: BookingWizardComponent },
  { path: 'payment/:id', component: PaymentComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'bookings', component: MyBookingsComponent },
  { path: 'wishlist', component: WishlistComponent },
  { path: '**', redirectTo: '' }
];
