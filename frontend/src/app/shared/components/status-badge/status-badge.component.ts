import { Component, computed, input } from '@angular/core';

type Status = 'active' | 'inactive' | 'pending' | 'approved' | 'rejected';

@Component({
  selector: 'app-status-badge',
  template: `<span class="status-badge" [class]="'status-badge status-' + status()">{{ label() }}</span>`
})
export class StatusBadgeComponent {
  readonly status = input<Status>('pending');
  readonly label = computed(() => ({ active: 'Activo', inactive: 'Inactivo', pending: 'Pendiente', approved: 'Aprobado', rejected: 'Rechazado' })[this.status()]);
}

