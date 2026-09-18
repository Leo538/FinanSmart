import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InvestmentProduct, InterestCalculationMethod, InterestPaymentFrequency } from '../../admin/investments/investment-products/models/investment-product.model';
import { InvestmentProductService } from '../../admin/investments/investment-products/services/investment-product.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { InvestmentSimulationResponse } from './models/investment-simulation.model';
import { InvestmentSimulationService } from './services/investment-simulation.service';
import { InvestmentApplicationService } from '../application/services/investment-application.service';

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
  private readonly router = inject(Router);
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
  readonly form = this.formBuilder.group({
    investmentProductId: ['', Validators.required], amount: [0, Validators.required], termDays: [0, Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  constructor() {
    this.productService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: products => { this.products.set(products.filter(product => product.isActive)); this.loadingProducts.set(false); },
      error: () => { this.loadingProducts.set(false); this.error.set('No se pudo conectar con el servidor.'); }
    });
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.selectedProduct.set(this.products().find(product => product.id === this.form.controls.investmentProductId.value) ?? null);
      if (this.result()) this.resultStale.set(true);
    });
  }

  simulate(): void {
    if (!this.isValid() || this.simulating()) { this.form.markAllAsTouched(); return; }
    this.error.set(''); this.simulating.set(true);
    const value = this.form.getRawValue();
    this.simulationService.simulate({ ...value, startDate: `${value.startDate}T00:00:00Z` }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => { this.result.set(result); this.resultStale.set(false); this.simulating.set(false); },
      error: error => { this.simulating.set(false); this.error.set(this.errorMessage(error)); }
    });
  }

  createApplication(): void {
    if (!this.result() || this.resultStale() || this.error() || this.creatingApplication()) return;
    const value = this.form.getRawValue();
    this.creatingApplication.set(true);
    this.applicationService.create({ ...value, startDate: `${value.startDate}T00:00:00Z` }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: application => { this.creatingApplication.set(false); this.router.navigate(['/client/investments/applications', application.id]); },
      error: () => { this.creatingApplication.set(false); this.error.set('Ocurrió un error al iniciar la solicitud de inversión.'); }
    });
  }

  isValid(): boolean {
    const product = this.selectedProduct(); const value = this.form.getRawValue();
    return this.form.valid && !!product && value.amount > 0 && value.amount >= product.minimumAmount &&
      (!product.maximumAmount || value.amount <= product.maximumAmount) && Number.isInteger(value.termDays) && value.termDays > 0 &&
      value.termDays >= product.minimumTermDays && (!product.maximumTermDays || value.termDays <= product.maximumTermDays);
  }
  amountError(): string {
    const product = this.selectedProduct(); const amount = this.form.controls.amount.value;
    if (amount <= 0) return 'Ingresa un monto válido.';
    if (!product) return '';
    if (amount < product.minimumAmount) return `El monto mínimo es ${this.money(product.minimumAmount)}.`;
    if (product.maximumAmount && amount > product.maximumAmount) return `El monto máximo es ${this.money(product.maximumAmount)}.`;
    return '';
  }
  termError(): string {
    const product = this.selectedProduct(); const termDays = this.form.controls.termDays.value;
    if (!Number.isInteger(termDays) || termDays <= 0) return 'Ingresa un plazo entero y positivo.';
    if (!product) return '';
    if (termDays < product.minimumTermDays) return `El plazo mínimo es ${product.minimumTermDays} días.`;
    if (product.maximumTermDays && termDays > product.maximumTermDays) return `El plazo máximo es ${product.maximumTermDays} días.`;
    return '';
  }
  money(value: number): string { return this.currencyFormatter.format(value); }
  percent(value: number): string { return `${this.percentageFormatter.format(value)} %`; }
  calculationMethodLabel(method: InterestCalculationMethod | string): string { return method === 'Compound' ? 'Interés compuesto' : 'Interés simple'; }
  paymentFrequencyLabel(frequency: InterestPaymentFrequency | string): string { return ({ AtMaturity: 'Al vencimiento', Monthly: 'Mensual', Quarterly: 'Trimestral', SemiAnnual: 'Semestral', Upfront: 'Anticipado' } as Record<string, string>)[frequency] ?? frequency; }
  paymentTypeLabel(paymentType: string): string { return ({ Interest: 'Pago de intereses', Maturity: 'Vencimiento', Upfront: 'Pago anticipado' } as Record<string, string>)[paymentType] ?? paymentType; }
  methodExplanation(method: InterestCalculationMethod | string): string { return method === 'Compound' ? 'Los intereses se calculan considerando la acumulación de intereses durante el plazo.' : 'Los intereses se calculan sobre el capital inicial durante el plazo de la inversión.'; }
  isPeriodic(frequency: InterestPaymentFrequency | string): boolean { return ['Monthly', 'Quarterly', 'SemiAnnual'].includes(frequency); }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    const message = typeof error.error === 'object' && error.error !== null && 'message' in error.error ? String(error.error.message).toLowerCase() : '';
    if (message.includes('rate')) return 'No existe una tasa disponible para el monto y plazo seleccionados.';
    if (error.status === 404 || message.includes('product')) return 'El producto seleccionado no está disponible.';
    if (error.status === 400) return 'Los parámetros no cumplen las condiciones configuradas para este producto.';
    return 'Ocurrió un error al calcular la inversión.';
  }
}
