import { Component, ElementRef, HostListener, inject, output, signal } from '@angular/core';

@Component({
  selector: 'app-admin-topbar',
  template: `<header class="topbar"><button class="menu-button btn btn-ghost btn-md" type="button" aria-label="Abrir o contraer menú" (click)="menuClicked.emit()"><span></span><span></span><span></span></button><p>Panel administrativo</p><div class="topbar-actions"><button class="icon-button" type="button" aria-label="Notificaciones"><span aria-hidden="true">○</span></button><div class="user-menu"><button class="user-button" type="button" [attr.aria-expanded]="isOpen()" aria-haspopup="menu" (click)="isOpen.set(!isOpen())"><span class="avatar">A</span><span class="user-name">Administrador</span><span aria-hidden="true">⌄</span></button>@if (isOpen()) { <div class="dropdown" role="menu"><button type="button" role="menuitem">Mi perfil</button><button type="button" role="menuitem">Configuración</button><button type="button" role="menuitem">Cerrar sesión</button></div> }</div></div></header>`,
  styles: [`.topbar{height:70px;background:var(--color-surface);border-bottom:1px solid var(--color-border);display:flex;align-items:center;padding:0 24px;gap:16px}.topbar>p{font-weight:700}.menu-button{width:40px;padding:0;flex-direction:column;gap:4px}.menu-button span{width:17px;height:2px;background:currentColor}.topbar-actions{margin-left:auto;display:flex;align-items:center;gap:10px}.icon-button{border:0;background:transparent;color:var(--color-text-secondary);font-size:1.4rem}.user-menu{position:relative}.user-button{display:flex;align-items:center;gap:8px;border:0;background:transparent;color:var(--color-text-primary);font-weight:600}.avatar{display:grid;place-items:center;width:32px;height:32px;border-radius:50%;background:var(--color-primary-light);color:var(--color-primary)}.dropdown{position:absolute;right:0;top:calc(100% + 10px);width:180px;padding:6px;background:var(--color-surface);border:1px solid var(--color-border);border-radius:var(--radius-sm);box-shadow:var(--shadow-md);z-index:50}.dropdown button{display:block;width:100%;padding:10px;text-align:left;background:transparent;border:0;border-radius:6px}.dropdown button:hover{background:var(--color-background)}@media(max-width:767px){.topbar{padding:0 16px}.user-name{display:none}}`]
})
export class AdminTopbarComponent {
  private readonly elementRef = inject(ElementRef);
  readonly menuClicked = output<void>();
  readonly isOpen = signal(false);
  @HostListener('document:click', ['$event']) onDocumentClick(event: Event) { if (!this.elementRef.nativeElement.contains(event.target)) this.isOpen.set(false); }
  @HostListener('document:keydown.escape') onEscape() { this.isOpen.set(false); }
}

