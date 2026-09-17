import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-client-dashboard',
  imports: [RouterLink, PageHeaderComponent],
  template: `<main class="page-container"><app-page-header title="Bienvenido a FinanSmart" subtitle="¿Qué deseas hacer?" /><section class="page-grid grid-3"><a class="quick-card card" routerLink="/client/credits/simulator"><h2>Simular un crédito</h2><p>Explora cuotas y condiciones según tus necesidades.</p><span>Comenzar →</span></a><a class="quick-card card" routerLink="/client/credits/comparison"><h2>Comparar sistemas</h2><p>Conoce las diferencias entre los sistemas de amortización.</p><span>Comparar →</span></a><article class="quick-card card disabled"><h2>Simular una inversión</h2><p>Próximamente podrás planificar tus inversiones.</p><span>Próximamente</span></article></section></main>`,
  styles: [`.quick-card{display:grid;gap:12px;padding:22px;transition:.18s}.quick-card:not(.disabled):hover{border-color:var(--color-accent);transform:translateY(-2px);box-shadow:var(--shadow-md)}.quick-card p{color:var(--color-text-secondary)}.quick-card span{color:var(--color-primary);font-weight:700}.quick-card.disabled{opacity:.65}`]
})
export class ClientDashboardComponent {}
