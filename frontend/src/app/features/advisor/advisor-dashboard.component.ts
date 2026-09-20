import { Component } from '@angular/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-advisor-dashboard',
  imports: [PageHeaderComponent],
  template: `<main class="page-container"><app-page-header title="Área del asesor" subtitle="Sesión iniciada correctamente como asesor." /><section class="card advisor-card"><h2>Bienvenido</h2><p>Desde este espacio podrás acceder a las funciones asignadas a tu rol cuando estén disponibles.</p></section></main>`,
  styles: [`.advisor-card{padding:24px;max-width:680px}.advisor-card h2{margin-top:0}.advisor-card p{color:var(--color-text-secondary)}`]
})
export class AdvisorDashboardComponent {}
