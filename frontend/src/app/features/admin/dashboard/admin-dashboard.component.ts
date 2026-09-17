import { Component } from '@angular/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-admin-dashboard',
  imports: [PageHeaderComponent, StatCardComponent, EmptyStateComponent],
  template: `<main class="page-container"><app-page-header title="Dashboard" subtitle="Resumen general de la plataforma financiera." /><section class="page-grid grid-4"><app-stat-card title="Tipos de crédito" value="4" secondaryText="Valores temporales" /><app-stat-card title="Tasas activas" value="4" secondaryText="Valores temporales" /><app-stat-card title="Clientes" value="0" secondaryText="Valores temporales" /><app-stat-card title="Simulaciones" value="0" secondaryText="Valores temporales" /></section><section class="dashboard-grid"><article class="card"><div class="card-header"><h2>Actividad reciente</h2></div><app-empty-state title="Aún no hay actividad registrada." /></article><article class="card"><div class="card-header"><h2>Distribución de productos</h2></div><app-empty-state title="Los datos aparecerán cuando existan operaciones." /></article></section></main>`,
  styles: [`.dashboard-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:20px;margin-top:20px}@media(max-width:767px){.dashboard-grid{grid-template-columns:1fr}}`]
})
export class AdminDashboardComponent {
  // Dashboard values will be loaded from the backend in a later phase.
}
