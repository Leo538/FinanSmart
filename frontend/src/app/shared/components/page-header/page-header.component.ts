import { Component, input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  template: `<header class="page-header"><div><h1>{{ title() }}</h1>@if (subtitle()) { <p>{{ subtitle() }}</p> }</div><div class="page-header-actions"><ng-content /></div></header>`,
  styles: [`.page-header{display:flex;align-items:flex-start;justify-content:space-between;gap:16px;margin-bottom:24px}.page-header p{color:var(--color-text-secondary);margin-top:6px}.page-header-actions{display:flex;gap:8px;flex-wrap:wrap}@media(max-width:767px){.page-header{flex-direction:column}}`]
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
}

