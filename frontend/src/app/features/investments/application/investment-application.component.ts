import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { InvestmentApplication, InvestmentApplicationStatus, InvestmentApplicationStep, UpdateInvestmentApplicant } from './models/investment-application.model';
import { InvestmentApplicationService } from './services/investment-application.service';

@Component({
  selector: 'app-investment-application',
  imports: [ReactiveFormsModule, DatePipe, EmptyStateComponent, LoadingSpinnerComponent, PageHeaderComponent],
  templateUrl: './investment-application.component.html',
  styleUrl: './investment-application.component.scss'
})
export class InvestmentApplicationComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly applicationService = inject(InvestmentApplicationService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly application = signal<InvestmentApplication | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly cancelling = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.group({
    firstName: ['', Validators.required], lastName: ['', Validators.required], identificationType: ['NationalId' as 'NationalId' | 'Passport', Validators.required],
    identificationNumber: ['', Validators.required], email: ['', [Validators.required, Validators.email]], phone: ['', Validators.required],
    birthDate: [''], address: ['', Validators.required], city: ['', Validators.required]
  });
  readonly steps: { key: InvestmentApplicationStep; label: string }[] = [
    { key: 'Investment', label: 'Inversión' }, { key: 'PersonalInformation', label: 'Datos personales' }, { key: 'Documents', label: 'Documentos' },
    { key: 'IdentityVerification', label: 'Validación de identidad' }, { key: 'Review', label: 'Revisión' }, { key: 'Confirmation', label: 'Confirmación' }
  ];

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.error.set('La solicitud no existe.'); this.loading.set(false); return; }
    this.applicationService.getById(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: application => { this.application.set(application); this.fillForm(application); this.loading.set(false); },
      error: error => { this.error.set(this.errorMessage(error)); this.loading.set(false); }
    });
  }

  saveApplicant(): void {
    const application = this.application();
    if (!application || this.isReadOnly() || this.saving()) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.error.set('');
    const value = this.form.getRawValue();
    const request: UpdateInvestmentApplicant = { ...value, birthDate: value.birthDate || null };
    this.applicationService.updateApplicant(application.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => { this.application.set(updated); this.saving.set(false); },
      error: error => { this.saving.set(false); this.error.set(this.errorMessage(error)); }
    });
  }

  cancel(): void {
    const application = this.application();
    if (!application || this.isReadOnly() || this.cancelling() || !confirm('¿Deseas cancelar esta solicitud de inversión?')) return;
    this.cancelling.set(true); this.error.set('');
    this.applicationService.cancel(application.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.application.update(current => current ? { ...current, status: 'Cancelled' } : current); this.cancelling.set(false); },
      error: error => { this.error.set(this.errorMessage(error)); this.cancelling.set(false); }
    });
  }

  goToSimulator(): void { this.router.navigate(['/client/investments/simulator']); }
  money(value: number): string { return this.currencyFormatter.format(value); }
  percent(value: number): string { return `${this.percentageFormatter.format(value)} %`; }
  methodLabel(method: string): string { return method === 'Compound' ? 'Interés compuesto' : 'Interés simple'; }
  frequencyLabel(frequency: string): string { return ({ AtMaturity: 'Al vencimiento', Monthly: 'Mensual', Quarterly: 'Trimestral', SemiAnnual: 'Semestral', Upfront: 'Anticipado' } as Record<string, string>)[frequency] ?? frequency; }
  statusLabel(status: InvestmentApplicationStatus): string { return ({ Draft: 'Borrador', PendingDocuments: 'Pendiente documentos', PendingIdentityVerification: 'Pendiente validación', ReadyForReview: 'Lista para revisión', Submitted: 'Enviada', Approved: 'Aprobada', Rejected: 'Rechazada', Cancelled: 'Cancelada' } as Record<string, string>)[status]; }
  isReadOnly(): boolean { return this.application()?.status === 'Cancelled'; }
  isComplete(step: InvestmentApplicationStep): boolean { const current = this.application()?.currentStep ?? 'PersonalInformation'; return this.stepIndex(step) < this.stepIndex(current); }
  isCurrent(step: InvestmentApplicationStep): boolean { return this.application()?.currentStep === step; }
  private stepIndex(step: InvestmentApplicationStep): number { return this.steps.findIndex(item => item.key === step); }
  private fillForm(application: InvestmentApplication): void {
    this.form.patchValue({ firstName: application.applicantFirstName ?? '', lastName: application.applicantLastName ?? '', identificationType: application.identificationType ?? 'NationalId', identificationNumber: application.identificationNumber ?? '', email: application.email ?? '', phone: application.phone ?? '', birthDate: application.birthDate ?? '', address: application.address ?? '', city: application.city ?? '' });
  }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 404) return 'La solicitud no existe.';
    if (error.status === 400) return 'Revisa la información ingresada.';
    return 'Ocurrió un error al procesar la solicitud.';
  }
}
