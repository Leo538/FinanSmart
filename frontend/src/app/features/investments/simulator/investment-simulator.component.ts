import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../core/auth/services/auth-state.service';
import { InvestmentProduct, InterestCalculationMethod, InterestPaymentFrequency } from '../../admin/investments/investment-products/models/investment-product.model';
import { InvestmentProductService } from '../../admin/investments/investment-products/services/investment-product.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { InvestmentSimulationResponse } from './models/investment-simulation.model';
import { InvestmentSimulationService } from './services/investment-simulation.service';
import { InvestmentApplicationService } from '../application/services/investment-application.service';
import { InvestmentRate } from '../../admin/investments/investment-rates/models/investment-rate.model';
import { InvestmentRateService } from '../../admin/investments/investment-rates/services/investment-rate.service';

@Component({
  selector: 'app-investment-simulator',
  imports: [ReactiveFormsModule, DatePipe, EmptyStateComponent, LoadingSpinnerComponent, PageHeaderComponent, StatCardComponent],
  templateUrl: './investment-simulator.component.html',
  styleUrl: './investment-simulator.component.scss'
})
export class InvestmentSimulatorComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly productService = inject(InvestmentProductService);
  private readonly simulationService = inject(InvestmentSimulationService);
  private readonly applicationService = inject(InvestmentApplicationService);
  private readonly rateService = inject(InvestmentRateService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly products = signal<InvestmentProduct[]>([]);
  readonly selectedProduct = signal<InvestmentProduct | null>(null);
  readonly loadingProducts = signal(true);
  readonly simulating = signal(false);
  readonly creatingApplication = signal(false);
  readonly result = signal<InvestmentSimulationResponse | null>(null);
  readonly resultStale = signal(false);
  readonly error = signal('');
  readonly applicableRate = signal<InvestmentRate | null>(null);
  readonly checkingRate = signal(false);
  readonly noApplicableRate = signal(false);
  readonly minimumStartDate = new Date().toISOString().slice(0, 10);
  readonly termUnit = signal<'days' | 'months'>('days');
  isPublic(): boolean { return this.router.url.startsWith('/simulators/'); }
  private rateTimer: ReturnType<typeof setTimeout> | null = null;
  readonly form = this.formBuilder.group({
    investmentProductId: ['', Validators.required], amount: [0, Validators.required], termDays: [0, Validators.required],
    startDate: [this.minimumStartDate, Validators.required]
  });

  constructor() {
    this.productService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: products => {
        const activeProducts = products.filter(product => product.isActive).map(product => ({
          ...product,
          minimumAmount: Number(product.minimumAmount), maximumAmount: product.maximumAmount === null ? null : Number(product.maximumAmount),
          minimumTermDays: Number(product.minimumTermDays), maximumTermDays: product.maximumTermDays === null ? null : Number(product.maximumTermDays)
        }));
        this.products.set(activeProducts);
        this.restorePendingSimulation();
        this.updateSelectedProduct(this.form.controls.investmentProductId.value);
        this.loadingProducts.set(false);
      },
      error: () => { this.loadingProducts.set(false); this.error.set('No se pudo conectar con el servidor.'); }
    });
    this.form.controls.investmentProductId.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(productId => {
      this.updateSelectedProduct(productId);
      if (this.result()) this.resultStale.set(true);
    });
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (this.result()) this.resultStale.set(true);
      this.scheduleApplicableRate();
    });
  }

  simulate(): void {
    if (!this.isValid() || !this.applicableRate() || this.simulating()) { this.form.markAllAsTouched(); return; }
    this.error.set(''); this.simulating.set(true);
    const value = this.form.getRawValue();
    this.simulationService.simulate({ ...value, termDays: this.normalizedTermDays(), startDate: `${value.startDate}T00:00:00Z` }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => { this.result.set(result); this.resultStale.set(false); this.simulating.set(false); },
      error: error => { this.simulating.set(false); this.error.set(this.errorMessage(error)); }
    });
  }

  createApplication(): void {
    if (!this.result() || this.resultStale() || this.error() || this.creatingApplication()) return;
    const value = this.form.getRawValue();
    if (!this.authState.hasRole('Client')) {
      sessionStorage.setItem('finansmart.pendingInvestmentSimulation', JSON.stringify({ ...value, termUnit: this.termUnit() }));
      void this.router.navigate(['/login'], { queryParams: { returnUrl: '/simulators/investment' } });
      return;
    }
    this.creatingApplication.set(true);
    this.applicationService.create({ ...value, termDays: this.normalizedTermDays(), startDate: `${value.startDate}T00:00:00Z` }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: application => { this.creatingApplication.set(false); this.router.navigate(['/client/investments/applications', application.id]); },
      error: () => { this.creatingApplication.set(false); this.error.set('Ocurrió un error al iniciar la solicitud de inversión.'); }
    });
  }

  isValid(): boolean {
    const product = this.selectedProduct(); const value = this.form.getRawValue();
    const termDays = this.normalizedTermDays();
    return this.form.valid && this.isStartDateValid() && !!product && Number.isFinite(value.amount) && Number.isFinite(termDays) && Number.isInteger(termDays);
  }
  isStartDateValid(): boolean { return this.form.controls.startDate.value >= this.minimumStartDate; }
  amountError(): string {
    const product = this.selectedProduct(); const amount = this.form.controls.amount.value;
    if (amount <= 0) return 'Ingresa un monto positivo.';
    if (!product) return '';
    if (amount < product.minimumAmount || (product.maximumAmount !== null && amount > product.maximumAmount)) return this.amountRangeMessage(product);
    return '';
  }
  termError(): string {
    const product = this.selectedProduct(); const termDays = this.normalizedTermDays();
    if (!Number.isInteger(termDays) || termDays <= 0) return 'Ingresa un plazo entero y positivo.';
    if (!product) return '';
    if (termDays < product.minimumTermDays || (product.maximumTermDays !== null && termDays > product.maximumTermDays)) return this.termRangeMessage(product);
    return '';
  }
  money(value: number): string { return this.currencyFormatter.format(value); }
  amountRange(product: InvestmentProduct): string { return product.maximumAmount === null ? `Desde ${this.money(product.minimumAmount)}` : `${this.money(product.minimumAmount)} - ${this.money(product.maximumAmount)}`; }
  termRange(product: InvestmentProduct): string {
    const minimum = this.termUnit() === 'months' ? Math.ceil(product.minimumTermDays / 30) : product.minimumTermDays;
    const maximum = product.maximumTermDays === null ? null : this.termUnit() === 'months' ? Math.floor(product.maximumTermDays / 30) : product.maximumTermDays;
    return maximum === null ? `Desde ${minimum} ${this.termUnitLabel()}` : `${minimum} - ${maximum} ${this.termUnitLabel()}`;
  }
  amountRangeMessage(product: InvestmentProduct): string { return product.maximumAmount === null ? `El monto mínimo permitido es ${this.money(product.minimumAmount)}.` : `El monto permitido para ${product.name} es de ${this.money(product.minimumAmount)} a ${this.money(product.maximumAmount)}.`; }
  termRangeMessage(product: InvestmentProduct): string { return `El plazo permitido es ${this.termRange(product)}.`; }
  percent(value: number): string { return `${this.percentageFormatter.format(value)} %`; }
  setTermUnit(unit: 'days' | 'months'): void {
    if (unit === this.termUnit()) return;
    const value = this.form.controls.termDays.value;
    this.form.controls.termDays.setValue(unit === 'months' ? Math.max(1, Math.ceil(value / 30)) : value * 30);
    this.termUnit.set(unit);
    this.updateSelectedProduct(this.form.controls.investmentProductId.value);
  }
  termUnitLabel(): string { return this.termUnit() === 'months' ? 'meses' : 'días'; }
  normalizedTermDays(): number { return this.termUnit() === 'months' ? this.form.controls.termDays.value * 30 : this.form.controls.termDays.value; }
  calculationMethodLabel(method: InterestCalculationMethod | string): string { return method === 'Compound' ? 'Interés compuesto' : 'Interés simple'; }
  paymentFrequencyLabel(frequency: InterestPaymentFrequency | string): string { return ({ AtMaturity: 'Al vencimiento', Monthly: 'Mensual', Quarterly: 'Trimestral', SemiAnnual: 'Semestral', Upfront: 'Anticipado' } as Record<string, string>)[frequency] ?? frequency; }
  paymentTypeLabel(paymentType: string): string { return ({ Interest: 'Pago de intereses', Maturity: 'Vencimiento', Upfront: 'Pago anticipado' } as Record<string, string>)[paymentType] ?? paymentType; }
  methodExplanation(method: InterestCalculationMethod | string): string { return method === 'Compound' ? 'Los intereses se calculan considerando la acumulación de intereses durante el plazo.' : 'Los intereses se calculan sobre el capital inicial durante el plazo de la inversión.'; }
  isPeriodic(frequency: InterestPaymentFrequency | string): boolean { return ['Monthly', 'Quarterly', 'SemiAnnual'].includes(frequency); }
  private updateSelectedProduct(productId: string): void {
    const product = this.products().find(item => item.id === productId) ?? null;
    this.selectedProduct.set(product);
    if (!product) {
      this.form.controls.amount.setValidators([Validators.required, Validators.min(0.01)]);
      this.form.controls.termDays.setValidators([Validators.required, Validators.min(1)]);
    } else {
      const amountValidators = [Validators.required, Validators.min(product.minimumAmount)];
      const minimumTerm = this.termUnit() === 'months' ? Math.ceil(product.minimumTermDays / 30) : product.minimumTermDays;
      const maximumTerm = product.maximumTermDays === null ? null : this.termUnit() === 'months' ? Math.floor(product.maximumTermDays / 30) : product.maximumTermDays;
      const termValidators = [Validators.required, Validators.min(minimumTerm)];
      if (product.maximumAmount !== null) amountValidators.push(Validators.max(product.maximumAmount));
      if (maximumTerm !== null) termValidators.push(Validators.max(maximumTerm));
      this.form.controls.amount.setValidators(amountValidators);
      this.form.controls.termDays.setValidators(termValidators);
      console.debug('[Investment simulator] Selected product limits', {
        productId: product.id, minimumAmount: product.minimumAmount, maximumAmount: product.maximumAmount,
        minimumTermDays: product.minimumTermDays, maximumTermDays: product.maximumTermDays
      });
    }
    this.form.controls.amount.updateValueAndValidity({ emitEvent: false });
    this.form.controls.termDays.updateValueAndValidity({ emitEvent: false });
    this.scheduleApplicableRate();
  }
  private restorePendingSimulation(): void {
    if (!this.authState.hasRole('Client')) return;
    const pending = sessionStorage.getItem('finansmart.pendingInvestmentSimulation');
    if (!pending) return;
    try {
      const value = JSON.parse(pending) as { investmentProductId?: string; amount?: number; termDays?: number; startDate?: string; termUnit?: 'days' | 'months' };
      if (value.investmentProductId && Number.isFinite(value.amount) && Number.isInteger(value.termDays) && value.startDate) {
        if (value.termUnit === 'months') this.termUnit.set('months');
        this.form.patchValue(value);
      }
    } catch { /* An invalid browser value must not interrupt the simulator. */ }
    sessionStorage.removeItem('finansmart.pendingInvestmentSimulation');
  }
  private scheduleApplicableRate(): void {
    if (this.rateTimer) clearTimeout(this.rateTimer);
    this.applicableRate.set(null); this.noApplicableRate.set(false);
    const product = this.selectedProduct(); const value = this.form.getRawValue(); const termDays = this.normalizedTermDays();
    if (!product || this.form.controls.amount.invalid || this.form.controls.termDays.invalid || !Number.isInteger(termDays)) { this.checkingRate.set(false); return; }
    this.checkingRate.set(true);
    this.rateTimer = setTimeout(() => this.rateService.getApplicableRate(product.id, value.amount, termDays).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: rate => { const current = this.form.getRawValue(); if (this.selectedProduct()?.id === product.id && current.amount === value.amount && this.normalizedTermDays() === termDays) { this.applicableRate.set(rate); this.noApplicableRate.set(false); } this.checkingRate.set(false); },
      error: error => { if (error.status === 404) this.noApplicableRate.set(true); this.checkingRate.set(false); }
    }), 250);
  }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error ? String(error.error.message).toLowerCase() : '';
    if (message.includes('rate')) return 'No existe una tasa disponible para el monto y plazo seleccionados.';
    if (error.status === 404 || message.includes('product')) return 'El producto seleccionado no está disponible.';
    if (error.status === 400) return 'Los parámetros no cumplen las condiciones configuradas para este producto.';
    return 'Ocurrió un error al calcular la inversión.';
  }
}
