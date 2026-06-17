import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { PackageDetailsComponent } from './pages/package-details/package-details';
import { ProfileComponent } from './pages/profile/profile';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'auth', component: AuthComponent },
  { path: 'package/:id', component: PackageDetailsComponent },
  { path: 'profile', component: ProfileComponent },
  { path: '**', redirectTo: '' }
];
