import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { CreditType } from '../../admin/credits/credit-types/models/credit-type.model';
import { CreditTypeService } from '../../admin/credits/credit-types/services/credit-type.service';
import { AmortizationComparisonSummary, CreditComparisonRequest, CreditComparisonResponse } from './models/credit-comparison.model';
import { CreditComparisonService } from './services/credit-comparison.service';

@Component({
  selector: 'app-credit-comparison',
  imports: [ReactiveFormsModule, PageHeaderComponent, LoadingSpinnerComponent, EmptyStateComponent],
  templateUrl: './credit-comparison.component.html',
  styleUrl: './credit-comparison.component.scss'
})
export class CreditComparisonComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly creditTypeService = inject(CreditTypeService);
  private readonly comparisonService = inject(CreditComparisonService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly creditTypes = signal<CreditType[]>([]);
  readonly loadingTypes = signal(true);
  readonly comparing = signal(false);
  readonly result = signal<CreditComparisonResponse | null>(null);
  readonly stale = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.group({
    creditTypeId: ['', Validators.required], amount: [0, Validators.required], termMonths: [0, Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required]
  });
  readonly selectedType = computed(() => this.creditTypes().find(type => type.id === this.form.controls.creditTypeId.value) ?? null);
  constructor() {
    this.creditTypeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: types => { this.creditTypes.set(types.filter(type => type.isActive)); this.loadingTypes.set(false); },
      error: () => { this.loadingTypes.set(false); this.error.set('No se pudo conectar con el servidor.'); }
    });
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => { if (this.result()) this.stale.set(true); });
  }
  compare(): void {
    if (!this.isValid() || this.comparing()) { this.form.markAllAsTouched(); return; }
    this.comparing.set(true); this.error.set('');
    const raw = this.form.getRawValue();
    const request: CreditComparisonRequest = { ...raw, startDate: raw.startDate + 'T00:00:00Z' };
    this.comparisonService.compare(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: response => { this.result.set(response); this.stale.set(false); this.comparing.set(false); },
      error: error => { this.comparing.set(false); this.error.set(this.errorMessage(error)); }
    });
  }
  isValid(): boolean {
    const type = this.selectedType(); const raw = this.form.getRawValue();
    return this.form.valid && !!type && raw.amount > 0 && raw.amount >= type.minimumAmount && raw.amount <= type.maximumAmount
      && Number.isInteger(raw.termMonths) && raw.termMonths >= type.minimumTermMonths && raw.termMonths <= type.maximumTermMonths;
  }
  amountError(): string { const type = this.selectedType(); const value = this.form.controls.amount.value; if (!type || value <= 0) return 'Ingresa un monto válido.'; if (value < type.minimumAmount) return `El monto mínimo para este crédito es ${this.money(type.minimumAmount)}.`; return value > type.maximumAmount ? `El monto máximo para este crédito es ${this.money(type.maximumAmount)}.` : ''; }
  termError(): string { const type = this.selectedType(); const value = this.form.controls.termMonths.value; if (!type || !Number.isInteger(value) || value <= 0) return 'Ingresa un plazo válido.'; return value < type.minimumTermMonths || value > type.maximumTermMonths ? `El plazo debe estar entre ${type.minimumTermMonths} y ${type.maximumTermMonths} meses.` : ''; }
  money(value: number): string { return this.currencyFormatter.format(value); }
  signedMoney(value: number): string { return (value >= 0 ? '+ ' : '- ') + this.money(Math.abs(value)); }
  percent(value: number): string { return this.percentageFormatter.format(value) + ' %'; }
  neutralDifference(label: string, value: number): string { return value === 0 ? `No existe diferencia en ${label.toLowerCase()}.` : `El sistema Francés registra ${this.money(Math.abs(value))} ${value > 0 ? 'más' : 'menos'} en ${label.toLowerCase()}.`; }
  summaryRows(result: CreditComparisonResponse): { label: string; french: number; german: number; difference: number }[] {
    return [
      { label: 'Primera cuota', french: result.french.firstPayment, german: result.german.firstPayment, difference: result.firstPaymentDifference },
      { label: 'Última cuota', french: result.french.lastPayment, german: result.german.lastPayment, difference: result.lastPaymentDifference },
      { label: 'Intereses', french: result.french.totalInterest, german: result.german.totalInterest, difference: result.interestDifference },
      { label: 'Cargos', french: result.french.totalCharges, german: result.german.totalCharges, difference: result.chargesDifference },
      { label: 'Total a pagar', french: result.french.totalPayment, german: result.german.totalPayment, difference: result.totalPaymentDifference }
    ];
  }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error ? String(error.error.message).toLowerCase() : '';
    if (message.includes('rate')) return 'No existe una tasa vigente para el tipo de crédito seleccionado.';
    if (error.status === 404 || message.includes('inactive')) return 'El tipo de crédito seleccionado no está disponible.';
    if (error.status === 400) return 'Los parámetros no cumplen las condiciones del tipo de crédito.';
    return 'Ocurrió un error al procesar la comparación.';
  }
}
