import { Component, input, output, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Institution } from '../../features/admin/institution/models/institution.model';

@Component({
  selector: 'app-admin-sidebar',
  imports: [RouterLink, RouterLinkActive],
  template: `<aside class="sidebar" [class.collapsed]="collapsed()" [class.mobile-open]="mobileOpen()"><div class="brand">@if (institution()?.logoUrl && !logoFailed()) { <img class="brand-logo" [src]="institution()!.logoUrl" [alt]="institution()!.name + ' logo'" (error)="logoFailed.set(true)" /> } @else { <span class="brand-mark">{{ initials() }}</span> }<span class="brand-name">{{ institution()?.name || 'FINANSMART' }}</span><button class="mobile-close btn btn-ghost btn-sm" type="button" aria-label="Cerrar menú" (click)="navigated.emit()">×</button></div><nav aria-label="Navegación administrativa">@for (section of menu; track section.label) { <p class="nav-section">{{ section.label }}</p>@for (item of section.items; track item.path) { <a [routerLink]="item.path" routerLinkActive="active" (click)="navigated.emit()"><span class="nav-icon" aria-hidden="true"></span><span>{{ item.label }}</span></a> }}</nav></aside>`,
  styles: [`.sidebar{width:250px;background:var(--color-primary-dark);color:#d7e5ef;min-height:100dvh;padding:16px 12px;transition:width .2s ease;overflow-y:auto;z-index:30}.brand{height:50px;display:flex;align-items:center;gap:10px;color:#fff;font-weight:800;letter-spacing:.04em;margin:0 4px 20px}.brand-mark,.brand-logo{display:grid;place-items:center;width:30px;height:30px;border-radius:8px;flex:0 0 auto;background:var(--color-accent);color:#fff}.brand-logo{object-fit:cover}.brand-name{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.sidebar nav{display:grid;gap:3px}.nav-section{font-size:.68rem;font-weight:800;letter-spacing:.08em;color:#94adbd;margin:17px 10px 5px;white-space:nowrap}.sidebar a{height:42px;display:flex;align-items:center;gap:12px;border-radius:8px;padding:0 10px;color:#d7e5ef;font-size:.9rem;white-space:nowrap}.sidebar a:hover,.sidebar a.active{background:rgb(255 255 255 / 12%);color:#fff}.nav-icon{width:12px;height:12px;border:1.5px solid currentColor;border-radius:3px;flex:0 0 auto}.collapsed{width:76px}.collapsed .brand-name,.collapsed .nav-section,.collapsed a span:last-child{display:none}.collapsed .brand{justify-content:center}.collapsed a{justify-content:center}.mobile-close{display:none;margin-left:auto;color:#fff}@media(max-width:767px){.sidebar{position:fixed;left:0;top:0;transform:translateX(-100%);width:270px;box-shadow:var(--shadow-md)}.sidebar.mobile-open{transform:translateX(0)}.mobile-close{display:inline-flex}} `]
})
export class AdminSidebarComponent {
  readonly collapsed = input(false);
  readonly mobileOpen = input(false);
  readonly institution = input<Institution | null>(null);
  readonly navigated = output<void>();
  readonly logoFailed = signal(false);
  readonly menu = [
    { label: 'GENERAL', items: [{ label: 'Dashboard', path: '/admin/dashboard' }] },
    { label: 'CRÉDITOS', items: [{ label: 'Tipos de crédito', path: '/admin/credits/types' }, { label: 'Tasas', path: '/admin/credits/rates' }, { label: 'Cargos', path: '/admin/credits/charges' }, { label: 'Simulador', path: '/admin/credits/simulator' }, { label: 'Comparación', path: '/admin/credits/comparison' }] },
    { label: 'INVERSIONES', items: [{ label: 'Productos', path: '/admin/investments/products' }, { label: 'Tasas', path: '/admin/investments/rates' }] },
    { label: 'GESTIÓN', items: [{ label: 'Clientes', path: '/admin/clients' }, { label: 'Reportes', path: '/admin/reports' }] },
    { label: 'SISTEMA', items: [{ label: 'Institución', path: '/admin/institution' }, { label: 'Usuarios', path: '/admin/users' }] }
  ];

  initials(): string {
    return (this.institution()?.name || 'FinanSmart')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0])
      .join('')
      .toUpperCase();
  }
}
