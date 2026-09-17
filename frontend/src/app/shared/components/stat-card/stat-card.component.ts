import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  template: `<article class="stat-card card"><p>{{ title() }}</p><strong>{{ value() }}</strong>@if (secondaryText()) { <small>{{ secondaryText() }}</small> }</article>`,
  styles: [`.stat-card{padding:20px;display:grid;gap:6px}.stat-card p,.stat-card small{color:var(--color-text-secondary)}.stat-card strong{font-size:2rem;line-height:1.15;color:var(--color-primary)}`]
})
export class StatCardComponent {
  readonly title = input.required<string>();
  readonly value = input.required<string | number>();
  readonly secondaryText = input<string>();
}

