import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  // Determine where to redirect unauthorized users
  // If they are logged in but not admin, maybe stay on home or show 403
  // For now, redirect to root
  return router.createUrlTree(['/']);
};
