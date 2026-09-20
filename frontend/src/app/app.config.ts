import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAppInitializer, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { authInterceptor } from './core/auth/interceptors/auth.interceptor';
import { AuthStateService } from './core/auth/services/auth-state.service';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideRouter(routes),
    provideAppInitializer(() => firstValueFrom(inject(AuthStateService).restoreSession()))
  ]
};
