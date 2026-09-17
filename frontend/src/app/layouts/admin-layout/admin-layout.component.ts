import { DOCUMENT } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminSidebarComponent } from './admin-sidebar.component';
import { AdminTopbarComponent } from './admin-topbar.component';
import { InstitutionStateService } from '../../core/services/institution-state.service';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, AdminSidebarComponent, AdminTopbarComponent],
  template: `<div class="admin-shell"><app-admin-sidebar [collapsed]="collapsed()" [mobileOpen]="mobileOpen()" [institution]="institutionState.currentInstitution()" (navigated)="closeMobileMenu()" /><div class="backdrop" [class.visible]="mobileOpen()" (click)="closeMobileMenu()"></div><div class="admin-content"><app-admin-topbar (menuClicked)="toggleMenu()" /><router-outlet /></div></div>`,
  styles: [`.admin-shell{min-height:100dvh;display:flex}.admin-content{min-width:0;flex:1}.backdrop{display:none}@media(max-width:767px){.backdrop{display:block;position:fixed;inset:0;background:rgb(11 41 66 / 45%);opacity:0;pointer-events:none;transition:.2s;z-index:20}.backdrop.visible{opacity:1;pointer-events:auto}}`]
})
export class AdminLayoutComponent {
  private readonly document = inject(DOCUMENT);
  readonly institutionState = inject(InstitutionStateService);
  readonly collapsed = signal(false);
  readonly mobileOpen = signal(false);
  constructor() {
    this.institutionState.loadInstitution();
    effect(() => this.document.body.classList.toggle('sidebar-mobile-open', this.mobileOpen()));
  }
  toggleMenu() { if (window.innerWidth < 768) this.mobileOpen.update(value => !value); else this.collapsed.update(value => !value); }
  closeMobileMenu() { this.mobileOpen.set(false); }
}
