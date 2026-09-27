import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `<section class="empty-state"><span class="empty-state-mark" aria-hidden="true"></span><h3>{{ title() }}</h3>@if (description()) { <p>{{ description() }}</p> }<ng-content /></section>`
})
export class EmptyStateComponent {
  readonly title = input.required<string>();
  readonly description = input<string>();
}

