import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Institution } from '../../features/admin/institution/models/institution.model';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  applyInstitutionTheme(institution: Institution): void {
    const root = this.document.documentElement;
    if (institution.primaryColor) root.style.setProperty('--color-primary', institution.primaryColor);
    if (institution.secondaryColor) root.style.setProperty('--color-accent', institution.secondaryColor);
  }
}
