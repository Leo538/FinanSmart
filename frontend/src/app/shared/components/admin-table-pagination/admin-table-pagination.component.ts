import { Component, Input } from '@angular/core';
import { AdminTablePagination } from '../../utils/admin-table-pagination';

@Component({
  selector: 'app-admin-table-pagination',
  template: `
    <div class="pagination" aria-label="Paginación de la tabla">
      <span>{{ pagination.start(total) }}–{{ pagination.end(total) }} de {{ total }}</span>
      <div class="pagination-controls">
        <button class="btn btn-secondary btn-sm" type="button" (click)="pagination.previous()" [disabled]="pagination.page === 1">Anterior</button>
        <button class="btn btn-secondary btn-sm" type="button" (click)="pagination.next(total)" [disabled]="pagination.page >= pagination.pages(total)">Siguiente</button>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; width: 100%; box-sizing: border-box; }
    .pagination { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-top: 16px; padding: 15px 22px 18px; color: var(--text-secondary); font-size: .78rem; font-weight: 400; }
    .pagination-controls { display: flex; align-items: center; gap: 10px; }
    .pagination .btn { min-height: 30px; padding: 0 12px; border: 1px solid var(--border-strong); border-radius: 999px; background: transparent; color: var(--brand-primary); box-shadow: none; font-size: .78rem; }
    @media (max-width: 620px) { .pagination { align-items: flex-start; flex-direction: column; padding-inline: 16px; }.pagination-controls { justify-content: flex-end; width: 100%; } }
  `]
})
export class AdminTablePaginationComponent {
  @Input({ required: true }) pagination!: AdminTablePagination;
  @Input({ required: true }) total = 0;
}
