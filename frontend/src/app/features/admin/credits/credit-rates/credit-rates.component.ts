import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { StatCardComponent } from '../../../../shared/components/stat-card/stat-card.component';
import { CreditType } from '../credit-types/models/credit-type.model';
import { CreditTypeService } from '../credit-types/services/credit-type.service';
import { CreditRate, CreditRateFormData } from './models/credit-rate.model';
import { CreditRateService } from './services/credit-rate.service';
import { AdminTablePagination } from '../../../../shared/utils/admin-table-pagination';
import { AdminTablePaginationComponent } from '../../../../shared/components/admin-table-pagination/admin-table-pagination.component';

type TemporalStatus = 'current' | 'future' | 'expired' | 'inactive';
const positiveValidator: ValidatorFn = control => typeof control.value === 'number' && control.value > 0 ? null : { positive: true };
const dateRangeValidator: ValidatorFn = group => {
  const from = group.get('effectiveFrom')?.value;
  const to = group.get('effectiveTo')?.value;
  return from && to && to < from ? { dateRange: true } : null;
};

@Component({
  selector: 'app-credit-rates',
  imports: [ReactiveFormsModule, DatePipe, PageHeaderComponent, LoadingSpinnerComponent, EmptyStateComponent, StatusBadgeComponent, StatCardComponent, AdminTablePaginationComponent],
  templateUrl: './credit-rates.component.html',
  styleUrl: './credit-rates.component.scss'
})
export class CreditRatesComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly creditRateService = inject(CreditRateService);
  private readonly creditTypeService = inject(CreditTypeService);
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly rates = signal<CreditRate[]>([]);
  readonly creditTypes = signal<CreditType[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly saving = signal(false);
  readonly modalOpen = signal(false);
  readonly editingRate = signal<CreditRate | null>(null);
  readonly confirmTarget = signal<CreditRate | null>(null);
  readonly selectedCreditTypeId = signal('');
  readonly selectedTemporalStatus = signal<'all' | TemporalStatus>('all');
  readonly feedback = signal('');
  readonly feedbackType = signal<'success' | 'error'>('success');
  readonly tablePagination = new AdminTablePagination();
  readonly form = this.formBuilder.group({
    creditTypeId: ['', Validators.required],
    annualInterestRate: [0, [Validators.required, positiveValidator]],
    effectiveFrom: [''],
    effectiveTo: [''],
    sourceDate: [''],
    sourceUrl: [''],
    isActive: [true]
  }, { validators: dateRangeValidator });
  readonly filteredRates = computed(() => this.rates().filter(rate =>
    (!this.selectedCreditTypeId() || rate.creditTypeId === this.selectedCreditTypeId())
    && (this.selectedTemporalStatus() === 'all' || this.temporalStatus(rate) === this.selectedTemporalStatus())
  ));
  readonly activeRates = computed(() => this.rates().filter(rate => rate.isActive).length);
  readonly currentTypeCount = computed(() => new Set(this.rates().filter(rate => this.temporalStatus(rate) === 'current').map(rate => rate.creditTypeId)).size);
  readonly futureRates = computed(() => this.rates().filter(rate => this.temporalStatus(rate) === 'future').length);

  pagedRates(): readonly CreditRate[] { return this.tablePagination.slice(this.filteredRates()); }

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.creditRateService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: rates => { this.rates.set(rates); this.loading.set(false); },
      error: () => { this.loadError.set(true); this.loading.set(false); }
    });
    this.creditTypeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: types => this.creditTypes.set(types) });
  }

  openCreate(): void {
    this.editingRate.set(null);
    this.form.reset({ creditTypeId: '', annualInterestRate: 0, effectiveFrom: '', effectiveTo: '', sourceDate: '', sourceUrl: '', isActive: true });
    this.modalOpen.set(true);
  }

  openEdit(rate: CreditRate): void {
    this.editingRate.set(rate);
    this.form.reset({
      creditTypeId: rate.creditTypeId,
      annualInterestRate: rate.annualInterestRate,
      effectiveFrom: rate.effectiveFrom ? this.toDateInput(rate.effectiveFrom) : '',
      effectiveTo: rate.effectiveTo ? this.toDateInput(rate.effectiveTo) : '',
      sourceDate: rate.sourceDate ? this.toDateInput(rate.sourceDate) : '',
      sourceUrl: rate.sourceUrl ?? '',
      isActive: rate.isActive
    });
    this.modalOpen.set(true);
  }

  availableCreditTypes(): CreditType[] {
    const editingTypeId = this.editingRate()?.creditTypeId;
    return this.creditTypes().filter(type => type.isActive || type.id === editingTypeId);
  }

  closeForm(): void { if (!this.saving()) this.modalOpen.set(false); }
  closeConfirmation(): void { if (!this.saving()) this.confirmTarget.set(null); }

  save(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const value = this.form.getRawValue();
    const data: CreditRateFormData = {
      creditTypeId: value.creditTypeId,
      annualInterestRate: value.annualInterestRate,
      effectiveFrom: value.effectiveFrom ? this.toUtcDate(value.effectiveFrom) : null,
      effectiveTo: value.effectiveTo ? this.toUtcDate(value.effectiveTo) : null,
      sourceDate: value.sourceDate ? this.toUtcDate(value.sourceDate) : null,
      sourceUrl: value.sourceUrl.trim() || null,
      isActive: value.isActive
    };
    const editing = this.editingRate();
    const request = editing ? this.creditRateService.update(editing.id, data) : this.creditRateService.create(data);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.setFeedback(editing ? 'Tasa de interés actualizada correctamente.' : 'Tasa de interés creada correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }

  requestDeactivation(rate: CreditRate): void { this.confirmTarget.set(rate); }
  deactivate(): void {
    const target = this.confirmTarget();
    if (!target || this.saving()) return;
    this.saving.set(true);
    this.creditRateService.delete(target.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.confirmTarget.set(null); this.setFeedback('Tasa de interés desactivada correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }

  reactivate(rate: CreditRate): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.creditRateService.update(rate.id, { creditTypeId: rate.creditTypeId, annualInterestRate: rate.annualInterestRate, effectiveFrom: rate.effectiveFrom, effectiveTo: rate.effectiveTo, sourceDate: rate.sourceDate, sourceUrl: rate.sourceUrl, isActive: true }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.setFeedback('Tasa de interés reactivada correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }

  temporalStatus(rate: CreditRate): TemporalStatus {
    if (!rate.isActive) return 'inactive';
    if (!rate.effectiveFrom) return 'current';
    const today = new Date().toISOString().slice(0, 10);
    if (this.toDateInput(rate.effectiveFrom) > today) return 'future';
    if (rate.effectiveTo && this.toDateInput(rate.effectiveTo) < today) return 'expired';
    return 'current';
  }
  temporalLabel(rate: CreditRate): string { return ({ current: 'Vigente', future: 'Futura', expired: 'Vencida', inactive: 'Inactiva' })[this.temporalStatus(rate)]; }
  formatRate(rate: number): string { return this.percentageFormatter.format(rate) + ' %'; }
  clearFilters(): void { this.selectedCreditTypeId.set(''); this.selectedTemporalStatus.set('all'); }
  setCreditTypeFilter(event: Event): void { this.selectedCreditTypeId.set((event.target as HTMLSelectElement).value); }
  setStatusFilter(event: Event): void { this.selectedTemporalStatus.set((event.target as HTMLSelectElement).value as 'all' | TemporalStatus); }
  private toUtcDate(value: string): string { return value + 'T00:00:00Z'; }
  private toDateInput(value: string): string { return value.slice(0, 10); }
  private setFeedback(message: string, type: 'success' | 'error'): void { this.feedback.set(message); this.feedbackType.set(type); }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 400) {
      const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error
        ? String(error.error.message).toLowerCase()
        : '';
      return message.includes('inactive')
        ? 'No se puede asignar una nueva tasa a un tipo de crédito inactivo.'
        : 'Revisa los datos ingresados.';
    }
    if (error.status === 404) return 'El tipo de crédito seleccionado ya no existe.';
    if (error.status === 409) return 'El período de vigencia se superpone con otra tasa registrada.';
    return 'Ocurrió un error al procesar la solicitud.';
  }
}
