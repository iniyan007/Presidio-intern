import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { PackageDetailsComponent } from './pages/package-details/package-details';
import { ProfileComponent } from './pages/profile/profile';
import { MyBookingsComponent } from './pages/my-bookings/my-bookings';
import { BookingWizardComponent } from './pages/booking-wizard/booking-wizard';
import { PaymentComponent } from './pages/payment/payment';
import { WishlistComponent } from './pages/wishlist/wishlist';
import { AgencyProfileComponent } from './pages/agency-profile/agency-profile';
import { ApplyAgencyComponent } from './pages/apply-agency/apply-agency';
import { VerifyEmailComponent } from './pages/verify-email/verify-email';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password';
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboard';
import { AdminAgenciesComponent } from './pages/admin-agencies/admin-agencies';
import { adminGuard } from './guards/admin.guard';
import { packagerGuard } from './guards/packager.guard';
import { AgencyDashboardComponent } from './pages/agency-dashboard/agency-dashboard';
import { ManageBookingsComponent } from './pages/manage-bookings/manage-bookings';
import { CreatePackageComponent } from './pages/create-package/create-package';
import { ChatComponent } from './pages/chat/chat';
import { ManagePackagesComponent } from './pages/manage-packages/manage-packages';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'auth', component: AuthComponent, canActivate: [guestGuard] },
  { path: 'verify-email', component: VerifyEmailComponent, canActivate: [guestGuard] },
  { path: 'package/:id', component: PackageDetailsComponent },
  { path: 'package/:id/book', component: BookingWizardComponent },
  { path: 'payment/:id', component: PaymentComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'bookings', component: MyBookingsComponent },
  { path: 'wishlist', component: WishlistComponent },
  { path: 'agency/dashboard', component: AgencyDashboardComponent, canActivate: [packagerGuard] },
  { path: 'agency/manage-packages', component: ManagePackagesComponent, canActivate: [packagerGuard] },
  { path: 'agency/manage-bookings', component: ManageBookingsComponent, canActivate: [packagerGuard] },
  { path: 'agency/create-package', component: CreatePackageComponent, canActivate: [packagerGuard] },
  { path: 'agency/edit-package/:id', component: CreatePackageComponent, canActivate: [packagerGuard] },
  { path: 'agency/:packageId', component: AgencyProfileComponent },
  { path: 'apply-agency', component: ApplyAgencyComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard] },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/packagers', component: AdminAgenciesComponent, canActivate: [adminGuard] },
  { path: 'chat', component: ChatComponent },
  { path: '**', redirectTo: '' }
];
