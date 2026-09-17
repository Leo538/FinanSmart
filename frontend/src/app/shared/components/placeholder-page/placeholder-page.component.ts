import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { PageHeaderComponent } from '../page-header/page-header.component';

@Component({
  selector: 'app-placeholder-page',
  imports: [PageHeaderComponent, EmptyStateComponent],
  template: `<main class="page-container"><app-page-header [title]="title" [subtitle]="subtitle" /><section class="card"><app-empty-state [title]="'Próximamente'" [description]="'Esta sección estará disponible en una próxima fase.'" /></section></main>`
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);
  readonly title = this.route.snapshot.data['title'] ?? 'Módulo';
  readonly subtitle = this.route.snapshot.data['subtitle'] ?? 'Área en preparación';
}

