import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { CreditType } from '../../admin/credits/credit-types/models/credit-type.model';
import { CreditTypeService } from '../../admin/credits/credit-types/services/credit-type.service';
import { AmortizationInstallment, AmortizationSystem, CreditSimulationRequest, CreditSimulationResponse } from './models/credit-simulation.model';
import { CreditSimulationService } from './services/credit-simulation.service';

@Component({
  selector: 'app-credit-simulator',
  imports: [ReactiveFormsModule, DatePipe, PageHeaderComponent, LoadingSpinnerComponent, EmptyStateComponent, StatCardComponent],
  templateUrl: './credit-simulator.component.html',
  styleUrl: './credit-simulator.component.scss'
})
export class CreditSimulatorComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly creditTypeService = inject(CreditTypeService);
  private readonly simulationService = inject(CreditSimulationService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly creditTypes = signal<CreditType[]>([]);
  readonly loadingTypes = signal(true);
  readonly simulating = signal(false);
  readonly result = signal<CreditSimulationResponse | null>(null);
  readonly resultStale = signal(false);
  readonly error = signal('');
  readonly expandedInstallment = signal<number | null>(null);
  readonly form = this.formBuilder.group({
    creditTypeId: ['', Validators.required], amount: [0, Validators.required], termMonths: [0, Validators.required],
    amortizationSystem: ['French' as AmortizationSystem, Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required]
  });
  readonly selectedType = computed(() => this.creditTypes().find(type => type.id === this.form.controls.creditTypeId.value) ?? null);
  constructor() {
    this.creditTypeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: types => { this.creditTypes.set(types.filter(type => type.isActive)); this.loadingTypes.set(false); },
      error: () => { this.loadingTypes.set(false); this.error.set('No se pudo conectar con el servidor.'); }
    });
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (this.result()) this.resultStale.set(true);
    });
  }
  selectSystem(system: AmortizationSystem): void { this.form.controls.amortizationSystem.setValue(system); }
  simulate(): void {
    if (!this.isValid() || this.simulating()) { this.form.markAllAsTouched(); return; }
    this.error.set(''); this.simulating.set(true);
    const value = this.form.getRawValue();
    const request: CreditSimulationRequest = { ...value, startDate: value.startDate + 'T00:00:00Z' };
    this.simulationService.simulate(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => { this.result.set(result); this.resultStale.set(false); this.simulating.set(false); this.expandedInstallment.set(null); },
      error: error => { this.simulating.set(false); this.error.set(this.errorMessage(error)); }
    });
  }
  isValid(): boolean {
    const type = this.selectedType(); const value = this.form.getRawValue();
    return this.form.valid && !!type && value.amount > 0 && value.amount >= type.minimumAmount && value.amount <= type.maximumAmount
      && Number.isInteger(value.termMonths) && value.termMonths >= type.minimumTermMonths && value.termMonths <= type.maximumTermMonths;
  }
  amountError(): string {
    const type = this.selectedType(); const amount = this.form.controls.amount.value;
    if (!type || amount <= 0) return 'Ingresa un monto válido.';
    if (amount < type.minimumAmount) return `El monto mínimo para este crédito es ${this.money(type.minimumAmount)}.`;
    if (amount > type.maximumAmount) return `El monto máximo para este crédito es ${this.money(type.maximumAmount)}.`;
    return '';
  }
  termError(): string {
    const type = this.selectedType(); const term = this.form.controls.termMonths.value;
    if (!type || !Number.isInteger(term) || term <= 0) return 'Ingresa un plazo válido.';
    if (term < type.minimumTermMonths || term > type.maximumTermMonths) return `El plazo debe estar entre ${type.minimumTermMonths} y ${type.maximumTermMonths} meses.`;
    return '';
  }
  money(value: number): string { return this.currencyFormatter.format(value); }
  percent(value: number): string { return this.percentageFormatter.format(value) + ' %'; }
  systemLabel(system: AmortizationSystem): string { return system === 'French' ? 'Sistema Francés' : 'Sistema Alemán'; }
  systemDescription(system: AmortizationSystem): string { return system === 'French' ? 'La cuota financiera se mantiene prácticamente constante. Al inicio se paga más interés y menos capital.' : 'El abono a capital es constante y las cuotas disminuyen progresivamente.'; }
  toggleCharges(installment: AmortizationInstallment): void { this.expandedInstallment.update(current => current === installment.installmentNumber ? null : installment.installmentNumber); }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error ? String(error.error.message).toLowerCase() : '';
    if (message.includes('rate')) return 'No existe una tasa vigente para el tipo de crédito seleccionado.';
    if (error.status === 404 || message.includes('inactive')) return 'El tipo de crédito seleccionado no está disponible.';
    if (error.status === 400) return 'Los parámetros del crédito no cumplen las condiciones configuradas.';
    return 'Ocurrió un error al procesar la simulación.';
  }
}
