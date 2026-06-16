import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth';
import { DashboardComponent } from './pages/dashboard/dashboard';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'auth', component: AuthComponent },
  { path: '**', redirectTo: '' }
];
