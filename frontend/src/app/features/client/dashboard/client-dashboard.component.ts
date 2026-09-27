import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AuthStateService } from '../../../core/auth/services/auth-state.service';
import { InvestmentApplication, InvestmentApplicationStatus } from '../../investments/application/models/investment-application.model';
import { InvestmentApplicationService } from '../../investments/application/services/investment-application.service';

@Component({
  selector: 'app-client-dashboard',
  imports: [RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './client-dashboard.component.html',
  styleUrl: './client-dashboard.component.scss'
})
export class ClientDashboardComponent {
  private readonly service = inject(InvestmentApplicationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly authState = inject(AuthStateService);
  readonly applications = signal<InvestmentApplication[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  constructor() { this.loadApplications(); }

  loadApplications(): void {
    this.loading.set(true);
    this.error.set(false);
    this.service.getMine().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: applications => {
        this.applications.set(applications.slice(0, 5));
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  pendingCount(): number {
    return this.applications().filter(application => [
      'PendingDocuments', 'PendingIdentityVerification', 'ReadyForReview', 'Submitted'
    ].includes(application.status)).length;
  }

  approvedCount(): number {
    return this.applications().filter(application => application.status === 'Approved').length;
  }

  statusLabel(status: InvestmentApplicationStatus): string {
    return ({
      Draft: 'Borrador', PendingDocuments: 'Pendiente de documentos',
      PendingIdentityVerification: 'Pendiente de identidad', ReadyForReview: 'Lista para enviar',
      Submitted: 'En revisión', Approved: 'Aprobada', Rejected: 'Rechazada', Cancelled: 'Cancelada'
    } as Record<InvestmentApplicationStatus, string>)[status];
  }

  statusClass(status: InvestmentApplicationStatus): string {
    if (status === 'Approved') return 'state--approved';
    if (status === 'Rejected' || status === 'Cancelled') return 'state--rejected';
    if (status === 'Submitted' || status === 'ReadyForReview') return 'state--pending';
    return 'state--neutral';
  }
}
