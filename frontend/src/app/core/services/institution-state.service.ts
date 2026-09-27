import { Injectable, inject, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, shareReplay, tap } from 'rxjs/operators';
import { Institution } from '../../features/admin/institution/models/institution.model';
import { InstitutionService } from '../../features/admin/institution/services/institution.service';
import { ThemeService } from './theme.service';

@Injectable({ providedIn: 'root' })
export class InstitutionStateService {
  private readonly institutionService = inject(InstitutionService);
  private readonly themeService = inject(ThemeService);
  private loadRequest?: Observable<Institution | null>;
  readonly currentInstitution = signal<Institution | null>(null);
  readonly loaded = signal(false);

  loadInstitution(): Observable<Institution | null> {
    if (this.loaded()) return of(this.currentInstitution());
    if (!this.loadRequest) {
      this.loadRequest = this.institutionService.getAll().pipe(
        map(institutions => {
          const activeInstitution = institutions.find(institution => institution.isActive) ?? null;
          return {
            activeInstitution,
            managedInstitution: activeInstitution ?? institutions[0] ?? null
          };
        }),
        tap(({ activeInstitution }) => {
          this.setInstitution(activeInstitution);
          this.loaded.set(true);
        }),
        map(({ managedInstitution }) => managedInstitution),
        catchError(() => {
          this.setInstitution(null);
          this.loaded.set(true);
          return of(null);
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.loadRequest;
  }

  setInstitution(institution: Institution | null): void {
    const activeInstitution = institution?.isActive ? institution : null;
    this.currentInstitution.set(activeInstitution);
    if (activeInstitution) this.themeService.applyInstitutionTheme(activeInstitution);
    else this.themeService.resetInstitutionTheme();
  }
}
