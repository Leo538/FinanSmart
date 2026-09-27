import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InstitutionStateService } from '../../../core/services/institution-state.service';
import { AdminDashboard } from './models/admin-dashboard.model';
import { AdminDashboardService } from './services/admin-dashboard.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [DatePipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent {
  private readonly api = inject(AdminDashboardService);
  private readonly destroy = inject(DestroyRef);
  readonly institution = inject(InstitutionStateService);
  readonly dashboard = signal<AdminDashboard | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal('');

  constructor() { this.loadDashboard(); }

  loadDashboard() {
    this.isLoading.set(true);
    this.error.set('');
    this.api.getDashboard().pipe(takeUntilDestroyed(this.destroy)).subscribe({
      next: dashboard => { this.dashboard.set(dashboard); this.isLoading.set(false); },
      error: () => { this.error.set('No se pudo conectar con el servidor.'); this.isLoading.set(false); }
    });
  }

  money(value: number) {
    return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value || 0);
  }

  status(status: string) {
    return ({
      Draft: 'Borrador', PendingDocuments: 'Pendiente de documentos',
      PendingIdentityVerification: 'Pendiente de identidad', ReadyForReview: 'Lista para enviar',
      Submitted: 'Pendiente de revisión', Approved: 'Aprobada', Rejected: 'Rechazada', Cancelled: 'Cancelada'
    } as Record<string, string>)[status] ?? status;
  }

  width(count: number) {
    const max = Math.max(...(this.dashboard()?.investmentApplicationsByStatus.map(item => item.count) ?? [0]));
    return max ? count / max * 100 : 0;
  }

  share(count: number) {
    const total = this.dashboard()?.investmentSummary.applicationsCount ?? 0;
    return total ? Math.round(count / total * 100) : 0;
  }
}
