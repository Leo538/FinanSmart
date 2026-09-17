import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CreditType, CreditTypeFormData } from './models/credit-type.model';
import { CreditTypeService } from './services/credit-type.service';

const rangeValidator = (minimumControl: string, maximumControl: string, errorName: string): ValidatorFn =>
  group => {
    const minimum = group.get(minimumControl)?.value;
    const maximum = group.get(maximumControl)?.value;
    return minimum !== null && maximum !== null && maximum < minimum ? { [errorName]: true } : null;
  };

const positiveValidator: ValidatorFn = control =>
  typeof control.value === 'number' && control.value > 0 ? null : { positive: true };

@Component({
  selector: 'app-credit-types',
  imports: [ReactiveFormsModule, DatePipe, PageHeaderComponent, LoadingSpinnerComponent, EmptyStateComponent, StatusBadgeComponent],
  templateUrl: './credit-types.component.html',
  styleUrl: './credit-types.component.scss'
})
export class CreditTypesComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly creditTypeService = inject(CreditTypeService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });

  readonly creditTypes = signal<CreditType[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly saving = signal(false);
  readonly modalOpen = signal(false);
  readonly confirmTarget = signal<CreditType | null>(null);
  readonly editingCreditType = signal<CreditType | null>(null);
  readonly feedback = signal('');
  readonly feedbackType = signal<'success' | 'error'>('success');
  readonly form = this.formBuilder.group({
    name: ['', Validators.required],
    description: [''],
    minimumAmount: [0, [Validators.required, positiveValidator]],
    maximumAmount: [0, [Validators.required, positiveValidator]],
    minimumTermMonths: [0, [Validators.required, positiveValidator, Validators.pattern(/^[0-9]+$/)]],
    maximumTermMonths: [0, [Validators.required, positiveValidator, Validators.pattern(/^[0-9]+$/)]],
    isActive: [true]
  }, {
    validators: [
      rangeValidator('minimumAmount', 'maximumAmount', 'amountRange'),
      rangeValidator('minimumTermMonths', 'maximumTermMonths', 'termRange')
    ]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.creditTypeService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: creditTypes => {
        this.creditTypes.set(creditTypes);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set(true);
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.editingCreditType.set(null);
    this.form.reset({
      name: '',
      description: '',
      minimumAmount: 0,
      maximumAmount: 0,
      minimumTermMonths: 0,
      maximumTermMonths: 0,
      isActive: true
    });
    this.modalOpen.set(true);
  }

  openEdit(creditType: CreditType): void {
    this.editingCreditType.set(creditType);
    this.form.reset({
      name: creditType.name,
      description: creditType.description ?? '',
      minimumAmount: creditType.minimumAmount,
      maximumAmount: creditType.maximumAmount,
      minimumTermMonths: creditType.minimumTermMonths,
      maximumTermMonths: creditType.maximumTermMonths,
      isActive: creditType.isActive
    });
    this.modalOpen.set(true);
  }

  closeForm(): void {
    if (!this.saving()) this.modalOpen.set(false);
  }

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const rawData = this.form.getRawValue();
    const data: CreditTypeFormData = {
      ...rawData,
      description: rawData.description.trim() || null
    };
    const editing = this.editingCreditType();
    const request = editing
      ? this.creditTypeService.update(editing.id, data)
      : this.creditTypeService.create(data);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.modalOpen.set(false);
        this.setFeedback(editing ? 'Tipo de crédito actualizado correctamente.' : 'Tipo de crédito creado correctamente.', 'success');
        this.load();
      },
      error: error => {
        this.saving.set(false);
        this.setFeedback(this.getErrorMessage(error), 'error');
      }
    });
  }

  requestDeactivation(creditType: CreditType): void {
    this.confirmTarget.set(creditType);
  }

  closeConfirmation(): void {
    this.confirmTarget.set(null);
  }

  deactivate(): void {
    const target = this.confirmTarget();
    if (!target || this.saving()) return;

    this.saving.set(true);
    this.creditTypeService.delete(target.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.confirmTarget.set(null);
        this.setFeedback('Tipo de crédito desactivado correctamente.', 'success');
        this.load();
      },
      error: error => {
        this.saving.set(false);
        this.setFeedback(this.getErrorMessage(error), 'error');
      }
    });
  }

  reactivate(creditType: CreditType): void {
    if (this.saving()) return;

    this.saving.set(true);
    const { id, createdAt, updatedAt, ...data } = creditType;
    this.creditTypeService.update(id, { ...data, isActive: true }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.setFeedback('Tipo de crédito reactivado correctamente.', 'success');
        this.load();
      },
      error: error => {
        this.saving.set(false);
        this.setFeedback(this.getErrorMessage(error), 'error');
      }
    });
  }

  formatAmount(amount: number): string {
    return this.currencyFormatter.format(amount);
  }

  private setFeedback(message: string, type: 'success' | 'error'): void {
    this.feedback.set(message);
    this.feedbackType.set(type);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 400) return 'Revisa los datos ingresados.';
    if (error.status === 404) return 'El tipo de crédito ya no existe.';
    if (error.status === 409) return 'Ya existe un tipo de crédito con ese nombre.';
    return 'Ocurrió un error al procesar la solicitud.';
  }
}
