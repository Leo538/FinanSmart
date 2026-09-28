export class AdminTablePagination {
  readonly pageSize = 10;
  page = 1;
  private previousItems?: readonly unknown[];

  slice<T>(items: readonly T[]): readonly T[] {
    if (items !== this.previousItems) {
      this.page = 1;
      this.previousItems = items;
    }
    this.page = Math.min(Math.max(1, this.page), this.pages(items.length));
    const start = (this.page - 1) * this.pageSize;
    return items.slice(start, start + this.pageSize);
  }

  pages(total: number): number { return Math.max(1, Math.ceil(total / this.pageSize)); }
  start(total: number): number { return total ? (Math.min(this.page, this.pages(total)) - 1) * this.pageSize + 1 : 0; }
  end(total: number): number { return Math.min(Math.min(this.page, this.pages(total)) * this.pageSize, total); }
  previous(): void { this.page = Math.max(1, this.page - 1); }
  next(total: number): void { this.page = Math.min(this.pages(total), this.page + 1); }
  reset(): void { this.page = 1; }
}
