import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnDestroy, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { InvestmentApplication, SourceOfFunds } from '../../../investments/application/models/investment-application.model';
import { InvestmentApplicationDocument } from '../../../investments/application/models/investment-application-document.model';
import { InvestmentIdentityVerification } from '../../../investments/application/models/investment-identity-verification.model';
import { InvestmentApplicationService } from '../../../investments/application/services/investment-application.service';
import { InvestmentApplicationDocumentService } from '../../../investments/application/services/investment-application-document.service';
import { InvestmentIdentityVerificationService } from '../../../investments/application/services/investment-identity-verification.service';

interface FilePreview { title: string; url: string; isImage: boolean; }

@Component({
  selector: 'app-investment-applications',
  standalone: true,
  imports: [FormsModule, DatePipe, RouterLink, PageHeaderComponent],
  templateUrl: './investment-applications.component.html',
  styleUrl: './investment-applications.component.scss'
})
export class InvestmentApplicationsComponent implements OnDestroy {
  private readonly applicationsService = inject(InvestmentApplicationService);
  private readonly documentsService = inject(InvestmentApplicationDocumentService);
  private readonly identityService = inject(InvestmentIdentityVerificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly thumbnailUrls = new Map<string, string>();

  readonly items = signal<InvestmentApplication[]>([]);
  readonly selected = signal<InvestmentApplication | null>(null);
  readonly documents = signal<InvestmentApplicationDocument[]>([]);
  readonly identity = signal<InvestmentIdentityVerification | null>(null);
  readonly thumbnails = signal<Record<string, string>>({});
  readonly selfieUrl = signal<string | null>(null);
  readonly preview = signal<FilePreview | null>(null);
  query = '';
  status = '';
  notes = '';
  page = 1;
  readonly pageSize = 8;

  constructor() {
    this.applicationsService.getAll().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(items => {
      this.items.set(items);
      const id = this.route.snapshot.paramMap.get('id');
      const application = id ? items.find(item => item.id === id) ?? null : null;
      this.selected.set(application);
      if (application) this.loadRelated(application.id);
    });
  }

  ngOnDestroy(): void {
    this.thumbnailUrls.forEach(url => URL.revokeObjectURL(url));
    const selfieUrl = this.selfieUrl();
    if (selfieUrl) URL.revokeObjectURL(selfieUrl);
    const preview = this.preview();
    if (preview) URL.revokeObjectURL(preview.url);
  }

  filtered(): InvestmentApplication[] {
    const term = this.query.trim().toLowerCase();
    return this.items().filter(item =>
      (!this.status || item.status === this.status) &&
      (!term || `${item.applicationNumber} ${item.applicantFirstName ?? ''} ${item.applicantLastName ?? ''} ${item.investmentProductName}`.toLowerCase().includes(term))
    );
  }

  paged(): InvestmentApplication[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  }

  pages(): number { return Math.max(1, Math.ceil(this.filtered().length / this.pageSize)); }
  previousPage(): void { this.page = Math.max(1, this.page - 1); }
  nextPage(): void { this.page = Math.min(this.pages(), this.page + 1); }
  resetPage(): void { this.page = 1; }
  money(value: number): string { return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value); }
  isImage(document: InvestmentApplicationDocument): boolean { return document.contentType.startsWith('image/'); }
  thumbnail(documentId: string): string | null { return this.thumbnails()[documentId] ?? null; }

  label(status: string): string {
    return ({ Submitted: 'En revisión', Approved: 'Aprobada', Rejected: 'Rechazada', Cancelled: 'Cancelada', Draft: 'Borrador', PendingDocuments: 'Pendiente de documentos', PendingIdentityVerification: 'Pendiente de identidad o declaraciones', ReadyForReview: 'Lista para enviar' } as Record<string, string>)[status] ?? status;
  }
  sourceLabel(source: SourceOfFunds | null): string {
    return ({ Salary: 'Sueldo', Savings: 'Ahorros', BusinessActivity: 'Actividad comercial', Other: 'Otros' } as Record<string, string>)[source ?? ''] ?? 'No registrado';
  }
  identityLabel(status: string): string { return ({ Pending: 'Pendiente', Captured: 'Capturada', Verified: 'Verificada', Rejected: 'Rechazada' } as Record<string, string>)[status] ?? status; }
  documentLabel(type: string): string { return ({ IdentityFront: 'Documento de identidad — frontal', IdentityBack: 'Documento de identidad — reverso', AdditionalDocument: 'Documento adicional' } as Record<string, string>)[type] ?? type; }

  decide(decision: 'Approve' | 'Reject'): void {
    const item = this.selected();
    if (!item || (decision === 'Reject' && !this.notes.trim())) return;
    this.applicationsService.review(item.id, decision, this.notes || null).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(updated => this.selected.set(updated));
  }

  openDocument(applicationId: string, document: InvestmentApplicationDocument): void {
    this.documentsService.download(applicationId, document.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(blob => {
      this.closePreview();
      this.preview.set({ title: document.originalFileName, url: URL.createObjectURL(blob), isImage: this.isImage(document) });
    });
  }

  closePreview(): void {
    const preview = this.preview();
    if (preview) URL.revokeObjectURL(preview.url);
    this.preview.set(null);
  }

  downloadDocument(applicationId: string, document: InvestmentApplicationDocument): void {
    this.documentsService.download(applicationId, document.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = globalThis.document.createElement('a');
      link.href = url;
      link.download = document.originalFileName;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  private loadRelated(id: string): void {
    this.documentsService.getAll(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(documents => {
      this.documents.set(documents);
      documents.filter(document => this.isImage(document)).forEach(document => this.loadThumbnail(id, document));
    });
    this.identityService.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: identity => {
        this.identity.set(identity);
        if (identity.selfieContentType?.startsWith('image/')) this.loadSelfie(id);
      }
    });
  }

  private loadThumbnail(applicationId: string, document: InvestmentApplicationDocument): void {
    this.documentsService.download(applicationId, document.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(blob => {
      const previous = this.thumbnailUrls.get(document.id);
      if (previous) URL.revokeObjectURL(previous);
      const url = URL.createObjectURL(blob);
      this.thumbnailUrls.set(document.id, url);
      this.thumbnails.update(thumbnails => ({ ...thumbnails, [document.id]: url }));
    });
  }

  private loadSelfie(applicationId: string): void {
    this.identityService.getSelfie(applicationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: blob => {
        const previous = this.selfieUrl();
        if (previous) URL.revokeObjectURL(previous);
        this.selfieUrl.set(URL.createObjectURL(blob));
      }
    });
  }
}
