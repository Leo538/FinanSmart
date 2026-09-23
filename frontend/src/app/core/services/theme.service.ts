import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Institution } from '../../features/admin/institution/models/institution.model';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  applyInstitutionTheme(institution: Institution): void {
    const root = this.document.documentElement;
    this.applyColor(root, '--primary', '--primary-dark', '--primary-soft', institution.primaryColor);
    this.applyColor(root, '--accent', '--accent-dark', '--accent-soft', institution.secondaryColor);
    this.document.title = institution.name.trim() || 'FinanSmart';
  }

  resetInstitutionTheme(): void {
    const root = this.document.documentElement;
    ['--primary', '--primary-dark', '--primary-soft', '--accent', '--accent-dark', '--accent-soft'].forEach(name => root.style.removeProperty(name));
    this.document.title = 'FinanSmart';
  }

  private applyColor(root: HTMLElement, colorVariable: string, darkVariable: string, softVariable: string, color: string | null): void {
    if (!color || !/^#[0-9a-f]{6}$/i.test(color)) {
      [colorVariable, darkVariable, softVariable].forEach(name => root.style.removeProperty(name));
      return;
    }

    root.style.setProperty(colorVariable, color);
    root.style.setProperty(darkVariable, this.mix(color, '#000000', .24));
    root.style.setProperty(softVariable, this.mix(color, '#ffffff', .88));
    if (colorVariable === '--primary') root.style.setProperty('--primary-contrast', this.isLight(color) ? '#14283b' : '#ffffff');
  }

  private mix(first: string, second: string, secondWeight: number): string {
    const from = first.slice(1).match(/.{2}/g)!.map(value => parseInt(value, 16));
    const to = second.slice(1).match(/.{2}/g)!.map(value => parseInt(value, 16));
    const channels = from.map((value, index) => Math.round(value * (1 - secondWeight) + to[index] * secondWeight));
    return `#${channels.map(value => value.toString(16).padStart(2, '0')).join('')}`;
  }

  private isLight(color: string): boolean {
    const [red, green, blue] = color.slice(1).match(/.{2}/g)!.map(value => parseInt(value, 16));
    return (red * 299 + green * 587 + blue * 114) / 1000 > 166;
  }
}
