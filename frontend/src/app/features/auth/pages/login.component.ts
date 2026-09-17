import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [RouterLink],
  template: `<section class="login-card card"><div class="login-brand"><span>F</span><strong>FINANSMART</strong><p>Tu plataforma financiera inteligente.</p></div><div class="form-group"><label class="form-label" for="email">Correo electrónico</label><input id="email" class="form-control" type="email" autocomplete="email" placeholder="nombre@correo.com" /></div><div class="form-group"><label class="form-label" for="password">Contraseña</label><input id="password" class="form-control" type="password" autocomplete="current-password" placeholder="••••••••" /></div><a class="forgot-link" routerLink="/forgot-password">¿Olvidaste tu contraseña?</a><button class="btn btn-primary btn-lg" type="button">Iniciar sesión</button><div class="development-access"><span>Acceso temporal de desarrollo</span><button class="btn btn-outline btn-md" type="button" routerLink="/admin/dashboard">Entrar como administrador</button><button class="btn btn-ghost btn-md" type="button" routerLink="/client/dashboard">Entrar como cliente</button></div></section>`,
  styles: [`.login-card{position:relative;z-index:1;width:min(100%,420px);padding:32px;display:grid;gap:18px}.login-brand{text-align:center;display:grid;justify-items:center;gap:7px;margin-bottom:6px}.login-brand>span{display:grid;place-items:center;width:46px;height:46px;border-radius:12px;background:var(--color-primary);color:#fff;font-size:1.35rem;font-weight:800}.login-brand strong{letter-spacing:.08em;color:var(--color-primary)}.login-brand p{font-size:.875rem;color:var(--color-text-secondary)}.forgot-link{font-size:.875rem;color:var(--color-primary);font-weight:600;justify-self:start}.development-access{display:grid;gap:8px;padding-top:16px;border-top:1px solid var(--color-border)}.development-access span{text-align:center;color:var(--color-text-muted);font-size:.75rem}.development-access .btn:last-child{color:var(--color-primary)}`]
})
export class LoginComponent {
  // Temporary development navigation; remove when real authentication is implemented.
}
