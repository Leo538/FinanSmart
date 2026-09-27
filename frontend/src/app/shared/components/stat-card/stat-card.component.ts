import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  template: `<article class="kpi-card"><p class="kpi-label">{{ title() }}</p><strong class="kpi-value">{{ value() }}</strong>@if (secondaryText()) { <small class="kpi-description">{{ secondaryText() }}</small> }</article>`,
  styles: [`.kpi-card{display:grid;align-content:start;gap:8px;min-height:124px;box-sizing:border-box;padding:22px;border:1px solid var(--border-default);border-radius:var(--radius-sm);background:var(--surface-panel);color:var(--text-primary);box-shadow:0 1px 3px rgb(15 23 42 / 6%)}.kpi-label{margin:0;color:var(--text-secondary);font-size:12px;font-weight:750;letter-spacing:.05em;text-transform:uppercase}.kpi-value{color:var(--text-primary);font-family:var(--font-family-heading);font-size:32px;font-weight:750;letter-spacing:-.045em;line-height:1.05;font-variant-numeric:tabular-nums}.kpi-description{color:var(--text-secondary);font-size:14px;line-height:1.4}`]
})
export class StatCardComponent {
  readonly title = input.required<string>();
  readonly value = input.required<string | number>();
  readonly secondaryText = input<string>();
}
