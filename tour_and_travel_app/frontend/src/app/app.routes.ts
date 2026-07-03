import { Routes } from '@angular/router';
import { adminGuard } from './guards/admin.guard';
import { packagerGuard } from './guards/packager.guard';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent) },
  { path: 'auth', loadComponent: () => import('./pages/auth/auth').then(m => m.AuthComponent), canActivate: [guestGuard] },
  { path: 'verify-email', loadComponent: () => import('./pages/verify-email/verify-email').then(m => m.VerifyEmailComponent), canActivate: [guestGuard] },
  { path: 'package/:id', loadComponent: () => import('./pages/package-details/package-details').then(m => m.PackageDetailsComponent) },
  { path: 'package/:id/book', loadComponent: () => import('./pages/booking-wizard/booking-wizard').then(m => m.BookingWizardComponent) },
  { path: 'payment/:id', loadComponent: () => import('./pages/payment/payment').then(m => m.PaymentComponent) },
  { path: 'profile', loadComponent: () => import('./pages/profile/profile').then(m => m.ProfileComponent) },
  { path: 'bookings', loadComponent: () => import('./pages/my-bookings/my-bookings').then(m => m.MyBookingsComponent) },
  { path: 'wishlist', loadComponent: () => import('./pages/wishlist/wishlist').then(m => m.WishlistComponent) },
  { path: 'agency/dashboard', loadComponent: () => import('./pages/agency-dashboard/agency-dashboard').then(m => m.AgencyDashboardComponent), canActivate: [packagerGuard] },
  { path: 'agency/manage-packages', loadComponent: () => import('./pages/manage-packages/manage-packages').then(m => m.ManagePackagesComponent), canActivate: [packagerGuard] },
  { path: 'agency/manage-bookings', loadComponent: () => import('./pages/manage-bookings/manage-bookings').then(m => m.ManageBookingsComponent), canActivate: [packagerGuard] },
  { path: 'agency/create-package', loadComponent: () => import('./pages/create-package/create-package').then(m => m.CreatePackageComponent), canActivate: [packagerGuard] },
  { path: 'agency/edit-package/:id', loadComponent: () => import('./pages/create-package/create-package').then(m => m.CreatePackageComponent), canActivate: [packagerGuard] },
  { path: 'agency/:packageId', loadComponent: () => import('./pages/agency-profile/agency-profile').then(m => m.AgencyProfileComponent) },
  { path: 'apply-agency', loadComponent: () => import('./pages/apply-agency/apply-agency').then(m => m.ApplyAgencyComponent) },
  { path: 'forgot-password', loadComponent: () => import('./pages/forgot-password/forgot-password').then(m => m.ForgotPasswordComponent), canActivate: [guestGuard] },
  { path: 'admin/dashboard', loadComponent: () => import('./pages/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardComponent), canActivate: [adminGuard] },
  { path: 'admin/packagers', loadComponent: () => import('./pages/admin-agencies/admin-agencies').then(m => m.AdminAgenciesComponent), canActivate: [adminGuard] },
  { path: 'chat', loadComponent: () => import('./pages/chat/chat').then(m => m.ChatComponent) },
  { path: '**', redirectTo: '' }
];
