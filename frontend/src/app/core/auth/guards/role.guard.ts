import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRole } from '../models/auth.models';
import { AuthStateService } from '../services/auth-state.service';

export const roleGuard = (roles: UserRole[]): CanActivateFn => () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  return roles.some(role => authState.hasRole(role))
    ? true
    : router.parseUrl(authState.redirectUrl());
};
