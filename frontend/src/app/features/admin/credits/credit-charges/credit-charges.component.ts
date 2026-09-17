import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
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
import { ChargeFrequency, ChargeType, CreditCharge, CreditChargeFormData } from './models/credit-charge.model';
import { CreditChargeService } from './services/credit-charge.service';

const positiveValidator: ValidatorFn = control => typeof control.value === 'number' && control.value > 0 ? null : { positive: true };

@Component({
  selector: 'app-credit-charges',
  imports: [ReactiveFormsModule, PageHeaderComponent, LoadingSpinnerComponent, EmptyStateComponent, StatusBadgeComponent, StatCardComponent],
  templateUrl: './credit-charges.component.html',
  styleUrl: './credit-charges.component.scss'
})
export class CreditChargesComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly chargeService = inject(CreditChargeService);
  private readonly creditTypeService = inject(CreditTypeService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly charges = signal<CreditCharge[]>([]);
  readonly creditTypes = signal<CreditType[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly saving = signal(false);
  readonly modalOpen = signal(false);
  readonly editingCharge = signal<CreditCharge | null>(null);
  readonly confirmTarget = signal<CreditCharge | null>(null);
  readonly feedback = signal('');
  readonly feedbackType = signal<'success' | 'error'>('success');
  readonly creditTypeFilter = signal('');
  readonly chargeTypeFilter = signal<'all' | ChargeType>('all');
  readonly frequencyFilter = signal<'all' | ChargeFrequency>('all');
  readonly activeFilter = signal<'all' | 'active' | 'inactive'>('all');
  readonly form = this.formBuilder.group({
    creditTypeId: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    chargeType: ['FixedAmount' as ChargeType, Validators.required],
    value: [0, [Validators.required, positiveValidator]],
    frequency: ['OneTime' as ChargeFrequency, Validators.required],
    isActive: [true]
  });
  readonly filteredCharges = computed(() => this.charges().filter(charge =>
    (!this.creditTypeFilter() || charge.creditTypeId === this.creditTypeFilter())
    && (this.chargeTypeFilter() === 'all' || charge.chargeType === this.chargeTypeFilter())
    && (this.frequencyFilter() === 'all' || charge.frequency === this.frequencyFilter())
    && (this.activeFilter() === 'all' || (this.activeFilter() === 'active' ? charge.isActive : !charge.isActive))
  ));
  readonly activeCount = computed(() => this.charges().filter(charge => charge.isActive).length);
  readonly monthlyCount = computed(() => this.charges().filter(charge => charge.frequency === 'Monthly').length);
  readonly typesWithCharges = computed(() => new Set(this.charges().map(charge => charge.creditTypeId)).size);

  constructor() { this.load(); }
  load(): void {
    this.loading.set(true); this.loadError.set(false);
    this.chargeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: charges => { this.charges.set(charges); this.loading.set(false); },
      error: () => { this.loadError.set(true); this.loading.set(false); }
    });
    this.creditTypeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: types => this.creditTypes.set(types) });
  }
  openCreate(): void {
    this.editingCharge.set(null);
    this.form.reset({ creditTypeId: '', name: '', description: '', chargeType: 'FixedAmount', value: 0, frequency: 'OneTime', isActive: true });
    this.modalOpen.set(true);
  }
  openEdit(charge: CreditCharge): void {
    this.editingCharge.set(charge);
    this.form.reset({ creditTypeId: charge.creditTypeId, name: charge.name, description: charge.description ?? '', chargeType: charge.chargeType, value: charge.value, frequency: charge.frequency, isActive: charge.isActive });
    this.modalOpen.set(true);
  }
  availableCreditTypes(): CreditType[] { const currentId = this.editingCharge()?.creditTypeId; return this.creditTypes().filter(type => type.isActive || type.id === currentId); }
  closeForm(): void { if (!this.saving()) this.modalOpen.set(false); }
  closeConfirmation(): void { if (!this.saving()) this.confirmTarget.set(null); }
  save(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const raw = this.form.getRawValue();
    const data: CreditChargeFormData = { ...raw, description: raw.description.trim() || null };
    const editing = this.editingCharge();
    const request = editing ? this.chargeService.update(editing.id, data) : this.chargeService.create(data);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.setFeedback(editing ? 'Cargo adicional actualizado correctamente.' : 'Cargo adicional creado correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }
  requestDeactivation(charge: CreditCharge): void { this.confirmTarget.set(charge); }
  deactivate(): void {
    const target = this.confirmTarget(); if (!target || this.saving()) return;
    this.saving.set(true);
    this.chargeService.delete(target.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.confirmTarget.set(null); this.setFeedback('Cargo adicional desactivado correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }
  reactivate(charge: CreditCharge): void {
    if (this.saving()) return;
    this.saving.set(true);
    const { id, creditTypeName, createdAt, updatedAt, ...data } = charge;
    this.chargeService.update(id, { ...data, isActive: true }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.saving.set(false); this.setFeedback('Cargo adicional reactivado correctamente.', 'success'); this.load(); },
      error: error => { this.saving.set(false); this.setFeedback(this.errorMessage(error), 'error'); }
    });
  }
  valueLabel(): string { return ({ FixedAmount: 'Valor ($)', PercentageOfPrincipal: 'Porcentaje sobre capital (%)', PercentageOfInstallment: 'Porcentaje sobre cuota (%)' })[this.form.controls.chargeType.value]; }
  valueHelp(): string { return ({ FixedAmount: 'Valor monetario que se cobrará al cliente.', PercentageOfPrincipal: 'Se calcula sobre el monto original solicitado.', PercentageOfInstallment: 'Se calcula sobre la cuota financiera antes de cargos.' })[this.form.controls.chargeType.value]; }
  applicationHelp(): string {
    const value = this.formatValue(this.form.controls.value.value, this.form.controls.chargeType.value);
    const type = this.form.controls.chargeType.value;
    const monthly = this.form.controls.frequency.value === 'Monthly';
    if (type === 'FixedAmount') return monthly ? `${value} se agregará a cada cuota.` : `${value} se cobrará una sola vez.`;
    const base = type === 'PercentageOfPrincipal' ? 'del monto original' : 'de la cuota financiera';
    return monthly ? `${value} ${base} se agregará a cada cuota.` : `${value} ${base} se cobrará una sola vez.`;
  }
  formatValue(value: number, type: ChargeType): string { return type === 'FixedAmount' ? this.currencyFormatter.format(value) : this.percentageFormatter.format(value) + ' %'; }
  chargeTypeLabel(type: ChargeType): string { return ({ FixedAmount: 'Valor fijo', PercentageOfPrincipal: 'Porcentaje sobre capital', PercentageOfInstallment: 'Porcentaje sobre cuota' })[type]; }
  frequencyLabel(frequency: ChargeFrequency): string { return frequency === 'Monthly' ? 'Mensual' : 'Una sola vez'; }
  clearFilters(): void { this.creditTypeFilter.set(''); this.chargeTypeFilter.set('all'); this.frequencyFilter.set('all'); this.activeFilter.set('all'); }
  setFilter(kind: 'credit' | 'type' | 'frequency' | 'active', event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    if (kind === 'credit') this.creditTypeFilter.set(value);
    if (kind === 'type') this.chargeTypeFilter.set(value as 'all' | ChargeType);
    if (kind === 'frequency') this.frequencyFilter.set(value as 'all' | ChargeFrequency);
    if (kind === 'active') this.activeFilter.set(value as 'all' | 'active' | 'inactive');
  }
  private setFeedback(message: string, type: 'success' | 'error'): void { this.feedback.set(message); this.feedbackType.set(type); }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 409) return 'Ya existe un cargo activo con ese nombre para este tipo de crédito.';
    if (error.status === 404) return 'El tipo de crédito seleccionado ya no existe.';
    if (error.status === 400) {
      const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error ? String(error.error.message).toLowerCase() : '';
      return message.includes('inactive') ? 'No se puede asignar un cargo a un tipo de crédito inactivo.' : 'Revisa la configuración del cargo.';
    }
    return 'Ocurrió un error al procesar la solicitud.';
  }
}
