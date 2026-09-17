import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-client-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `<div class="client-shell"><header><a class="client-brand" routerLink="/client/dashboard">FINANSMART</a><nav aria-label="Navegación de cliente"><a routerLink="/client/dashboard" routerLinkActive="active">Inicio</a><a routerLink="/client/credits/simulator" routerLinkActive="active">Simular crédito</a><a routerLink="/client/credits/comparison" routerLinkActive="active">Comparar sistemas</a><a routerLink="/client/simulations" routerLinkActive="active">Mis simulaciones</a></nav><span class="client-avatar" aria-label="Cliente">C</span></header><router-outlet /></div>`,
  styles: [`.client-shell{min-height:100dvh}.client-shell header{height:70px;display:flex;align-items:center;gap:28px;padding:0 max(24px,calc((100% - 1500px)/2));background:var(--color-surface);border-bottom:1px solid var(--color-border)}.client-brand{font-weight:800;color:var(--color-primary);letter-spacing:.04em}.client-shell nav{display:flex;gap:18px;flex:1}.client-shell nav a{color:var(--color-text-secondary);font-size:.9rem}.client-shell nav a.active{color:var(--color-primary);font-weight:700}.client-avatar{display:grid;place-items:center;width:32px;height:32px;border-radius:50%;background:var(--color-primary-light);color:var(--color-primary);font-weight:700}@media(max-width:767px){.client-shell header{padding:0 16px;gap:12px;overflow-x:auto}.client-shell nav{gap:14px;white-space:nowrap}.client-brand{font-size:.78rem}}`]
})
export class ClientLayoutComponent {}

