import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  template: `<main class="auth-shell"><div class="auth-glow glow-one"></div><div class="auth-glow glow-two"></div><router-outlet /></main>`,
  styles: [`.auth-shell{min-height:100dvh;display:grid;place-items:center;padding:24px;background:linear-gradient(135deg,var(--color-primary-dark),var(--color-primary));position:relative;overflow:hidden}.auth-glow{position:absolute;border-radius:50%;background:rgb(24 169 153 / 18%);filter:blur(2px)}.glow-one{width:420px;height:420px;top:-180px;right:-80px}.glow-two{width:300px;height:300px;bottom:-130px;left:-80px}`]
})
export class AuthLayoutComponent {}

