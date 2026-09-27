import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Institution } from '../../features/admin/institution/models/institution.model';
import { INSTITUTION_PALETTE } from '../theme/institution-palette';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  applyInstitutionTheme(institution: Institution): void {
    const root = this.document.documentElement;
    const primary = this.isValidHex(institution.primaryColor) ? institution.primaryColor! : INSTITUTION_PALETTE.primary;
    const secondary = this.isValidHex(institution.secondaryColor) ? institution.secondaryColor! : INSTITUTION_PALETTE.secondary;
    const background = this.isValidHex(institution.backgroundColor) ? institution.backgroundColor! : INSTITUTION_PALETTE.background;
    const hover = this.isValidHex(institution.hoverColor)
      ? institution.hoverColor!
      : primary.toLowerCase() === INSTITUTION_PALETTE.primary.toLowerCase()
        ? INSTITUTION_PALETTE.hover
        : this.mixColors(primary, '#000000', .24);
    const navy = this.mixColors(primary, '#001C2A', .78);
    const font = this.fontStack(institution.fontFamily);

    this.applyColor(root, '--primary', '--primary-dark', '--primary-soft', primary);
    root.style.setProperty('--primary-dark', hover);
    this.applyColor(root, '--accent', '--accent-dark', '--accent-soft', secondary);
    this.applyNavigationTokens(root, primary);
    root.style.setProperty('--brand-primary', 'var(--primary)');
    root.style.setProperty('--brand-primary-hover', 'var(--primary-dark)');
    root.style.setProperty('--brand-primary-active', 'var(--primary-dark)');
    root.style.setProperty('--hover-contrast', this.bestContrast(hover, '#ffffff', '#14283b'));
    root.style.setProperty('--brand-secondary', 'var(--accent)');
    root.style.setProperty('--brand-accent', 'var(--accent)');
    root.style.setProperty('--surface-page', background);
    root.style.setProperty('--surface-panel', this.mixColors(background, '#ffffff', .58));
    root.style.setProperty('--surface-subtle', this.mixColors(background, '#ffffff', .28));
    root.style.setProperty('--surface-dark', navy);
    root.style.setProperty('--landing-navy', navy);
    root.style.setProperty('--landing-teal', this.mixColors(primary, secondary, .7));
    root.style.setProperty('--landing-hover', hover);
    root.style.setProperty('--landing-accent', secondary);
    root.style.setProperty('--landing-accent-dark', this.mixColors(secondary, '#000000', .35));
    root.style.setProperty('--accent-soft', this.mixColors(secondary, '#ffffff', .82));
    root.style.setProperty('--font-family-base', font);
    root.style.setProperty('--font-family-heading', font);
    root.style.setProperty('--accent-contrast', this.bestContrast(secondary, '#ffffff', '#14283b'));
    this.document.title = institution.name.trim() || 'FinanSmart';
  }

  resetInstitutionTheme(): void {
    const root = this.document.documentElement;
    [
      '--primary', '--primary-dark', '--primary-soft', '--primary-contrast',
      '--accent', '--accent-dark', '--accent-soft', '--nav-bg', '--nav-bg-elevated',
      '--nav-border', '--nav-text', '--nav-text-muted', '--nav-active-bg',
      '--nav-active-text', '--nav-hover-bg', '--nav-indicator', '--topbar-bg', '--topbar-border',
      '--surface-dark', '--accent-contrast',
      '--surface-page', '--surface-panel', '--surface-subtle', '--landing-navy',
      '--landing-teal', '--landing-hover', '--landing-accent', '--landing-accent-dark',
      '--font-family-base', '--font-family-heading', '--brand-primary',
      '--brand-primary-hover', '--brand-primary-active', '--brand-secondary', '--brand-accent',
      '--hover-contrast'
    ].forEach(name => root.style.removeProperty(name));
    this.document.title = 'FinanSmart';
  }

  private applyNavigationTokens(root: HTMLElement, primary: string): void {
    // Keep navigation structural and legible even when the institutional color is very light.
    const navBackground = primary;
    const navElevated = this.mixColors(navBackground, '#000000', .16);
    const navActive = this.mixColors(navBackground, '#000000', .28);
    const navHover = this.mixColors(navBackground, '#ffffff', .08);
    const navText = this.bestContrast(navBackground, '#ffffff', '#14283b');
    const navMuted = this.mixColors(navText, navBackground, .31);

    root.style.setProperty('--nav-bg', navBackground);
    root.style.setProperty('--nav-bg-elevated', navElevated);
    root.style.setProperty('--nav-border', this.mixColors(navText, navBackground, .78));
    root.style.setProperty('--nav-text', navText);
    root.style.setProperty('--nav-text-muted', navMuted);
    root.style.setProperty('--nav-active-bg', navActive);
    root.style.setProperty('--nav-active-text', this.bestContrast(navActive, '#ffffff', '#14283b'));
    root.style.setProperty('--nav-hover-bg', navHover);
    root.style.setProperty('--nav-indicator', primary);
    root.style.setProperty('--topbar-bg', this.mixColors(primary, '#ffffff', .975));
    root.style.setProperty('--topbar-border', this.mixColors(primary, '#ffffff', .86));
  }

  private applyColor(root: HTMLElement, colorVariable: string, darkVariable: string, softVariable: string, color: string): void {
    root.style.setProperty(colorVariable, color);
    root.style.setProperty(darkVariable, this.mixColors(color, '#000000', .24));
    root.style.setProperty(softVariable, this.mixColors(color, '#ffffff', .88));
    if (colorVariable === '--primary') root.style.setProperty('--primary-contrast', this.bestContrast(color, '#ffffff', '#14283b'));
  }

  private isValidHex(color: string | null | undefined): color is string {
    return !!color && /^#[0-9a-f]{6}$/i.test(color);
  }

  private fontStack(value: string | null | undefined): string {
    if (value === 'Arial') return 'Arial, Helvetica, sans-serif';
    if (value === 'Georgia') return 'Georgia, Cambria, serif';
    return 'Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
  }

  private hexToRgb(color: string): [number, number, number] {
    const value = color.replace('#', '');
    return [parseInt(value.slice(0, 2), 16), parseInt(value.slice(2, 4), 16), parseInt(value.slice(4, 6), 16)];
  }

  private rgbToHex([red, green, blue]: [number, number, number]): string {
    return `#${[red, green, blue].map(channel => Math.round(channel).toString(16).padStart(2, '0')).join('')}`;
  }

  private mixColors(first: string, second: string, secondWeight: number): string {
    const from = this.hexToRgb(first);
    const to = this.hexToRgb(second);
    return this.rgbToHex([0, 1, 2].map(index => from[index] * (1 - secondWeight) + to[index] * secondWeight) as [number, number, number]);
  }

  private luminance(color: string): number {
    const channels = this.hexToRgb(color).map(channel => {
      const value = channel / 255;
      return value <= .03928 ? value / 12.92 : Math.pow((value + .055) / 1.055, 2.4);
    });
    return .2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2];
  }

  private contrast(first: string, second: string): number {
    const [lighter, darker] = [this.luminance(first), this.luminance(second)].sort((a, b) => b - a);
    return (lighter + .05) / (darker + .05);
  }

  private bestContrast(background: string, light: string, dark: string): string {
    return this.contrast(background, light) >= this.contrast(background, dark) ? light : dark;
  }
}
