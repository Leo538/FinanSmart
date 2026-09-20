import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, Observable, of, tap } from 'rxjs';
import { AuthResponse, CurrentUser, UserRole } from '../models/auth.models';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly authService = inject(AuthService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly router = inject(Router);
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly initialized = signal(false);

  restoreSession(): Observable<boolean> {
    if (!this.tokenStorage.getAccessToken()) {
      this.initialized.set(true);
      return of(false);
    }

    return this.authService.getCurrentUser().pipe(
      tap(user => this.currentUser.set(user)),
      map(() => true),
      catchError(() => {
        this.clearSession();
        return of(false);
      }),
      finalize(() => this.initialized.set(true))
    );
  }

  setSession(response: AuthResponse): void {
    this.tokenStorage.save(response.accessToken, response.refreshToken);
    this.currentUser.set(response.user);
    this.initialized.set(true);
  }

  updateCurrentUser(user: CurrentUser): void {
    this.currentUser.set(user);
    this.initialized.set(true);
  }

  clearSession(): void {
    this.tokenStorage.clear();
    this.currentUser.set(null);
    this.initialized.set(true);
  }

  logout(): Observable<void> {
    return this.authService.logout().pipe(
      catchError(() => of(void 0)),
      finalize(() => {
        this.clearSession();
        void this.router.navigateByUrl('/login');
      })
    );
  }

  hasRole(role: UserRole): boolean {
    return this.currentUser()?.roles.includes(role) ?? false;
  }

  redirectUrl(): string {
    if (this.hasRole('Admin')) return '/admin/dashboard';
    if (this.hasRole('Client')) return '/client/dashboard';
    if (this.hasRole('Advisor')) return '/advisor/dashboard';
    return '/login';
  }
}
