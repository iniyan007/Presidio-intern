import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { PackageDetailsComponent } from './pages/package-details/package-details';
import { ProfileComponent } from './pages/profile/profile';
import { MyBookingsComponent } from './pages/my-bookings/my-bookings';
import { BookingWizardComponent } from './pages/booking-wizard/booking-wizard';
import { PaymentComponent } from './pages/payment/payment';
import { WishlistComponent } from './pages/wishlist/wishlist';
import { PackagerProfileComponent } from './pages/packager-profile/packager-profile';
import { ApplyPackagerComponent } from './pages/apply-packager/apply-packager';
import { VerifyEmailComponent } from './pages/verify-email/verify-email';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password';
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboard';
import { AdminPackagersComponent } from './pages/admin-packagers/admin-packagers';
import { adminGuard } from './guards/admin.guard';
import { packagerGuard } from './guards/packager.guard';
import { PackagerDashboardComponent } from './pages/packager-dashboard/packager-dashboard';
import { ManageBookingsComponent } from './pages/manage-bookings/manage-bookings';
import { CreatePackageComponent } from './pages/create-package/create-package';
import { ChatComponent } from './pages/chat/chat';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'auth', component: AuthComponent },
  { path: 'verify-email', component: VerifyEmailComponent },
  { path: 'package/:id', component: PackageDetailsComponent },
  { path: 'package/:id/book', component: BookingWizardComponent },
  { path: 'payment/:id', component: PaymentComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'bookings', component: MyBookingsComponent },
  { path: 'wishlist', component: WishlistComponent },
  { path: 'packager/dashboard', component: PackagerDashboardComponent, canActivate: [packagerGuard] },
  { path: 'packager/manage-bookings', component: ManageBookingsComponent, canActivate: [packagerGuard] },
  { path: 'packager/create-package', component: CreatePackageComponent, canActivate: [packagerGuard] },
  { path: 'packager/edit-package/:id', component: CreatePackageComponent, canActivate: [packagerGuard] },
  { path: 'packager/:packageId', component: PackagerProfileComponent },
  { path: 'apply-packager', component: ApplyPackagerComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/packagers', component: AdminPackagersComponent, canActivate: [adminGuard] },
  { path: 'chat', component: ChatComponent },
  { path: '**', redirectTo: '' }
];
