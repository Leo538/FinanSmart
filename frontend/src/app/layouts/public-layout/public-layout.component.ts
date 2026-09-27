import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { InstitutionStateService } from '../../core/services/institution-state.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="public-shell">
      <header class="public-header">
        <a class="brand" routerLink="/simulators/home" [attr.aria-label]="(institution.currentInstitution()?.name || 'FinanSmart') + ', ir al inicio'">
          @if (institution.currentInstitution()?.logoUrl && !logoFailed()) { <img class="brand-logo" [src]="institution.currentInstitution()?.logoUrl" alt="" (error)="logoFailed.set(true)" /> }
          @else { <svg class="brand-symbol" aria-hidden="true" viewBox="0 0 32 36" fill="none"><path d="M3 31h26M7 28V19l4-3v12M14 28V8l4-3v23M22 28V13l4 3v12" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/><path d="M5 31c8-1 11-5 15-11 2-3 5-4 8-5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg> }
          <span>{{ institution.currentInstitution()?.name || 'FinanSmart' }}</span>
        </a>
        <nav aria-label="Navegación pública">
          <a routerLink="/simulators/credit" routerLinkActive="active">Créditos</a>
          <a routerLink="/simulators/comparison" routerLinkActive="active">Comparar</a>
          <a routerLink="/simulators/investment" routerLinkActive="active">Inversiones</a>
        </nav>
        <a class="header-cta" routerLink="/login">Comenzar <span aria-hidden="true">›</span></a>
      </header>
      <router-outlet />
    </div>
  `,
  styles: [`
    .public-shell { min-height: 100dvh; background: #fff; }
    .public-header { position: relative; z-index: 20; display: flex; align-items: center; justify-content: space-between; gap: 36px; height: 96px; padding: 0 9.75%; border-bottom: 1px solid rgb(195 228 230 / 12%); background: var(--landing-navy, #003347); color: #fff; font-family: var(--font-family-base); }
    .public-shell:has(app-public-home) .public-header { position: absolute; top: 0; right: 0; left: 0; background: transparent; }
    .brand { display: inline-flex; align-items: center; gap: 11px; flex: none; color: #fff; font-size: 1.83rem; font-weight: 750; letter-spacing: -.045em; white-space: nowrap; text-decoration: none; }
    .brand-symbol { width: 29px; height: 32px; flex: none; color: #fff; }
    .brand-logo { width: 36px; height: 36px; flex: none; border-radius: 9px; background: #fff; object-fit: contain; }
    nav { display: flex; align-items: center; justify-content: center; gap: clamp(22px, 3.2vw, 48px); margin: 0 auto; }
    nav a { color: #f3fbfb; font-size: 1.02rem; font-weight: 520; white-space: nowrap; text-decoration: none; transition: color .18s; }
    nav a:hover, nav a.active { color: var(--accent-soft, #a9e4e6); }
    .header-cta { display: inline-flex; flex: none; align-items: center; justify-content: center; gap: 18px; min-width: 171px; min-height: 56px; border: 1.5px solid #fff; border-radius: 999px; color: #fff; font-size: .96rem; font-weight: 600; white-space: nowrap; text-decoration: none; transition: background .18s, color .18s; }
    .header-cta:hover { background: #fff; color: var(--landing-navy, #003347); }.header-cta span { font-size: 1.65rem; line-height: .5; }
    @media (max-width: 1040px) { .public-header { gap: 20px; padding: 0 32px; }.brand { font-size: 1.4rem; }nav { gap: 20px; }nav a { font-size: .86rem; }.header-cta { min-width: 135px; min-height: 48px; font-size: .8rem; } }
    @media (max-width: 700px) { .public-header { height: 76px; padding: 0 20px; gap: 12px; }.brand { font-size: 1.15rem; }.brand-symbol { width: 24px; font-size: 1.45rem; }nav { justify-content: flex-start; gap: 16px; overflow-x: auto; scrollbar-width: none; }nav::-webkit-scrollbar { display: none; }nav a { font-size: .72rem; }.header-cta { min-width: auto; min-height: 37px; gap: 5px; padding: 0 11px; font-size: .7rem; } }
    @media (max-width: 490px) { .brand span:last-child { display: none; }.brand-symbol { width: 29px; } }
  `]
})
export class PublicLayoutComponent {
  readonly institution = inject(InstitutionStateService);
  readonly logoFailed = signal(false);

  constructor() { this.institution.loadInstitution(); }
}
