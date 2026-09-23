import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/services/auth.service';
import { AuthStateService } from '../../../core/auth/services/auth-state.service';
import { InstitutionStateService } from '../../../core/services/institution-state.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly institution = inject(InstitutionStateService);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly logoFailed = signal(false);
  readonly registered = signal(this.route.snapshot.queryParamMap.get('registered') === 'true');
  readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  initials(): string {
    return (this.institution.currentInstitution()?.name || 'FinanSmart').split(' ').filter(Boolean).slice(0, 2).map(part => part[0]).join('').toUpperCase();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.authService.login(this.form.getRawValue()).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: response => {
        this.authState.setSession(response);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const safeInvestmentReturn = response.user.roles.includes('Client') && returnUrl === '/simulators/investment';
        void this.router.navigateByUrl(safeInvestmentReturn ? returnUrl : this.authState.redirectUrl());
      },
      error: response => this.error.set(
        response.status === 401
          ? 'Correo o contraseña incorrectos.'
          : 'No fue posible iniciar sesión. Intenta nuevamente.'
      )
    });
  }
}
