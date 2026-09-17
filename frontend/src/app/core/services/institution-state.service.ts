import { Injectable, inject, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, shareReplay, tap } from 'rxjs/operators';
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
        map(institutions => institutions.find(institution => institution.isActive) ?? null),
        tap(institution => {
          this.setInstitution(institution);
          this.loaded.set(true);
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.loadRequest;
  }

  setInstitution(institution: Institution | null): void {
    this.currentInstitution.set(institution);
    if (institution) this.themeService.applyInstitutionTheme(institution);
  }
}
