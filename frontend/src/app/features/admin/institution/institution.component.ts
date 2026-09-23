import { Component, DestroyRef, inject, signal } from '@angular/core';
import { ReactiveFormsModule, Validators, NonNullableFormBuilder } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { Institution, InstitutionFormData } from './models/institution.model';
import { InstitutionService } from './services/institution.service';
import { InstitutionStateService } from '../../../core/services/institution-state.service';

@Component({
  selector: 'app-institution',
  imports: [ReactiveFormsModule, PageHeaderComponent, LoadingSpinnerComponent, StatusBadgeComponent],
  templateUrl: './institution.component.html',
  styleUrl: './institution.component.scss'
})
export class InstitutionComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly institutionService = inject(InstitutionService);
  private readonly institutionState = inject(InstitutionStateService);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly selectedInstitution = signal<Institution | null>(null);
  readonly feedback = signal('');
  readonly feedbackType = signal<'success' | 'error'>('success');
  readonly logoFailed = signal(false);
  readonly logoUploading = signal(false);
  readonly form = this.formBuilder.group({
    name: ['', Validators.required],
    ruc: ['', Validators.required],
    email: ['', Validators.email],
    phone: [''],
    address: [''],
    logoUrl: [''],
    primaryColor: ['#123B5D', Validators.pattern(/^(|#[0-9A-Fa-f]{6})$/)],
    secondaryColor: ['#18A999', Validators.pattern(/^(|#[0-9A-Fa-f]{6})$/)],
    isActive: [true]
  });
  readonly preview = signal(this.form.getRawValue());

  constructor() {
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.preview.set(this.form.getRawValue());
      this.logoFailed.set(false);
    });
    this.institutionState.loadInstitution().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: institution => {
        if (institution) this.loadForm(institution);
        this.loading.set(false);
      },
      error: () => {
        this.setFeedback('No se pudo conectar con el servidor.', 'error');
        this.loading.set(false);
      }
    });
  }

  setColor(control: 'primaryColor' | 'secondaryColor', event: Event): void {
    this.form.controls[control].setValue((event.target as HTMLInputElement).value);
  }

  uploadLogo(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    const institution = this.selectedInstitution();
    if (!file || !institution || this.logoUploading()) {
      if (!institution) this.setFeedback('Guarda primero la institución antes de cargar el logo.', 'error');
      return;
    }
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 2 * 1024 * 1024) {
      this.setFeedback('Selecciona una imagen JPG, PNG o WEBP de hasta 2 MB.', 'error');
      return;
    }
    this.logoUploading.set(true);
    this.institutionService.uploadLogo(institution.id, file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => {
        this.logoUploading.set(false);
        this.logoFailed.set(false);
        this.form.controls.logoUrl.setValue(updated.logoUrl ?? '');
        this.selectedInstitution.set(updated);
        this.institutionState.setInstitution(updated);
        this.setFeedback('Logo institucional actualizado.', 'success');
      },
      error: () => { this.logoUploading.set(false); this.setFeedback('No fue posible cargar el logo.', 'error'); }
    });
  }

  removeLogo(): void {
    const institution = this.selectedInstitution();
    if (!institution || this.logoUploading()) return;
    this.logoUploading.set(true);
    this.institutionService.deleteLogo(institution.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        const updated = { ...institution, logoUrl: null };
        this.logoUploading.set(false);
        this.logoFailed.set(false);
        this.form.controls.logoUrl.setValue('');
        this.selectedInstitution.set(updated);
        this.institutionState.setInstitution(updated);
        this.setFeedback('Logo eliminado. Se usarán las iniciales como respaldo.', 'success');
      },
      error: () => { this.logoUploading.set(false); this.setFeedback('No fue posible eliminar el logo.', 'error'); }
    });
  }

  initials(name: string): string {
    return name.trim().split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0]).join('').toUpperCase() || 'F';
  }

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.feedback.set('');
    const data: InstitutionFormData = this.form.getRawValue();
    const existing = this.selectedInstitution();

    if (existing) {
      this.institutionService.update(existing.id, data).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.handleSaved({ ...existing, ...data, updatedAt: new Date().toISOString() }),
        error: error => this.handleSaveError(error)
      });
      return;
    }

    this.institutionService.create(data).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: institution => this.handleSaved(institution),
      error: error => this.handleSaveError(error)
    });
  }

  private loadForm(institution: Institution): void {
    this.selectedInstitution.set(institution);
    this.form.patchValue({
      name: institution.name,
      ruc: institution.ruc,
      email: institution.email,
      phone: institution.phone,
      address: institution.address,
      logoUrl: institution.logoUrl ?? '',
      primaryColor: institution.primaryColor ?? '#123B5D',
      secondaryColor: institution.secondaryColor ?? '#18A999',
      isActive: institution.isActive
    });
  }

  private handleSaved(institution: Institution): void {
    this.selectedInstitution.set(institution);
    this.institutionState.setInstitution(institution);
    this.setFeedback('Configuración guardada correctamente.', 'success');
    this.saving.set(false);
  }

  private handleSaveError(error: HttpErrorResponse): void {
    this.setFeedback(this.getErrorMessage(error), 'error');
    this.saving.set(false);
  }

  private setFeedback(message: string, type: 'success' | 'error'): void {
    this.feedback.set(message);
    this.feedbackType.set(type);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) return 'Ya existe una institución con ese RUC.';
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    return 'No fue posible guardar la configuración.';
  }
}
