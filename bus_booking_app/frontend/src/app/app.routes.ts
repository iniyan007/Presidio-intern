import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { UserDashboard } from './pages/user-dashboard/user-dashboard';
import { OperatorDashboard } from './pages/operator-dashboard/operator-dashboard';
import { AdminDashboard } from './pages/admin-dashboard/admin-dashboard';
import { TripDetailsComponent } from './pages/trip-details/trip-details';
import { authGuard, roleGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'trip/:id', component: TripDetailsComponent, canActivate: [authGuard] },
  { 
    path: 'user/dashboard', 
    component: UserDashboard, 
    canActivate: [authGuard, roleGuard], 
    data: { role: 'User' } 
  },
  { 
    path: 'operator/dashboard', 
    component: OperatorDashboard, 
    canActivate: [authGuard, roleGuard], 
    data: { role: 'Operator' } 
  },
  { 
    path: 'admin/dashboard', 
    component: AdminDashboard, 
    canActivate: [authGuard, roleGuard], 
    data: { role: 'Admin' } 
  },
  { path: '**', redirectTo: '' }
];
