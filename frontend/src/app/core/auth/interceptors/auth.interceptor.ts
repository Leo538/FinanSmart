import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, finalize, switchMap, take, tap, throwError } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

const RETRIED_AFTER_REFRESH = new HttpContextToken<boolean>(() => false);
let refreshInProgress = false;
const refreshedAccessToken = new BehaviorSubject<string | false | null>(null);

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const tokenStorage = inject(TokenStorageService);
  const authService = inject(AuthService);
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const isTokenExcludedEndpoint = /\/api\/auth\/(login|refresh)$/.test(request.url);
  const isRefreshExcludedEndpoint = /\/api\/auth\/(login|refresh|logout)$/.test(request.url);
  const accessToken = tokenStorage.getAccessToken();
  const authenticatedRequest = !isTokenExcludedEndpoint && accessToken
    ? request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isRefreshExcludedEndpoint || request.context.get(RETRIED_AFTER_REFRESH)) {
        return throwError(() => error);
      }

      const refreshToken = tokenStorage.getRefreshToken();
      if (!refreshToken) return endSession(error, authState, router);

      if (refreshInProgress) {
        return refreshedAccessToken.pipe(
          filter((token): token is string | false => token !== null),
          take(1),
          switchMap(token => token
            ? next(request.clone({ context: request.context.set(RETRIED_AFTER_REFRESH, true), setHeaders: { Authorization: `Bearer ${token}` } }))
            : endSession(error, authState, router))
        );
      }

      refreshInProgress = true;
      refreshedAccessToken.next(null);
      return authService.refresh(refreshToken).pipe(
        tap(response => {
          authState.setSession(response);
          refreshedAccessToken.next(response.accessToken);
        }),
        switchMap(response => next(request.clone({ context: request.context.set(RETRIED_AFTER_REFRESH, true), setHeaders: { Authorization: `Bearer ${response.accessToken}` } }))),
        catchError(refreshError => {
          refreshedAccessToken.next(false);
          return endSession(refreshError, authState, router);
        }),
        finalize(() => { refreshInProgress = false; })
      );
    })
  );
};

function endSession(error: HttpErrorResponse, authState: AuthStateService, router: Router) {
  authState.clearSession();
  void router.navigateByUrl('/login');
  return throwError(() => error);
}
