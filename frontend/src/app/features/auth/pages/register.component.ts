import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/services/auth.service';
import { InstitutionStateService } from '../../../core/services/institution-state.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly institution = inject(InstitutionStateService);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly logoFailed = signal(false);
  readonly form = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.error.set('Las contraseñas no coinciden.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const { confirmPassword, ...request } = this.form.getRawValue();
    this.authService.registerClient(request).subscribe({
      next: () => void this.router.navigate(['/login'], { queryParams: { registered: 'true' } }),
      error: response => {
        this.loading.set(false);
        this.error.set(this.getErrorMessage(response.status));
      }
    });
  }

  initials(): string {
    return (this.institution.currentInstitution()?.name || 'FinanSmart').split(' ').filter(Boolean).slice(0, 2).map(word => word[0]).join('').toUpperCase();
  }

  private getErrorMessage(status: number): string {
    if (status === 409) return 'Ya existe una cuenta registrada con ese correo.';
    if (status === 400) return 'Revisa la información ingresada.';
    if (status === 0) return 'No se pudo conectar con el servidor.';
    return 'No fue posible crear la cuenta.';
  }
}
