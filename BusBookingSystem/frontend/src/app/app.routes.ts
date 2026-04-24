import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ProfileComponent } from './features/user/profile/profile';
import { BookingsComponent } from './features/user/bookings/bookings';
import { SearchResultsComponent } from './features/user/search-results/search-results';
import { SeatSelectionComponent } from './features/user/seat-selection/seat-selection';
import { DashboardComponent as OperatorDashboardComponent } from './features/operator/dashboard/dashboard';
import { BusesComponent as OperatorBusesComponent } from './features/operator/buses/buses';
import { TripsComponent } from './features/operator/trips/trips';
import { DashboardComponent as AdminDashboardComponent } from './features/admin/dashboard/dashboard';
import { OperatorsComponent } from './features/admin/operators/operators';
import { BusesComponent as AdminBusesComponent } from './features/admin/buses/buses';
import { RoutesComponent } from './features/operator/routes/routes';
import { AuthGuard } from './core/guards/auth-guard';
import { RoleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'search', component: SearchResultsComponent },
  
  // User Routes
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'bookings', component: BookingsComponent, canActivate: [AuthGuard] },
  { path: 'book/:tripId', component: SeatSelectionComponent, canActivate: [AuthGuard] },
  
  // Operator Routes
  { 
    path: 'operator', 
    canActivate: [AuthGuard, RoleGuard], 
    data: { roles: ['OPERATOR'] },
    children: [
      { path: 'dashboard', component: OperatorDashboardComponent },
      { path: 'buses', component: OperatorBusesComponent },
      { path: 'trips', component: TripsComponent },
      { path: 'routes', component: RoutesComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  
  // Admin Routes
  { 
    path: 'admin', 
    canActivate: [AuthGuard, RoleGuard], 
    data: { roles: ['ADMIN'] },
    children: [
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'operators', component: OperatorsComponent },
      { path: 'buses', component: AdminBusesComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  
  { path: '**', redirectTo: '' }
];
