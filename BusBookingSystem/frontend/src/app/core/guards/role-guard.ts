import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const RoleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const requiredRoles = route.data['roles'] as Array<string>;
  const userRole = authService.currentUser()?.role;

  if (userRole && requiredRoles.includes(userRole)) {
    return true;
  }

  return router.createUrlTree(['/']);
};
