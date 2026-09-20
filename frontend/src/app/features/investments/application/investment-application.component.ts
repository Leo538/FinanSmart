import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, OnDestroy, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { InvestmentApplication, InvestmentApplicationStatus, InvestmentApplicationStep, UpdateInvestmentApplicant } from './models/investment-application.model';
import { InvestmentApplicationService } from './services/investment-application.service';
import { InvestmentApplicationDocument, InvestmentDocumentRequirements, InvestmentDocumentType } from './models/investment-application-document.model';
import { InvestmentApplicationDocumentService } from './services/investment-application-document.service';
import { InvestmentIdentityVerification } from './models/investment-identity-verification.model';
import { InvestmentIdentityVerificationService } from './services/investment-identity-verification.service';

@Component({
  selector: 'app-investment-application',
  imports: [ReactiveFormsModule, DatePipe, EmptyStateComponent, LoadingSpinnerComponent, PageHeaderComponent],
  templateUrl: './investment-application.component.html',
  styleUrl: './investment-application.component.scss'
})
export class InvestmentApplicationComponent implements OnDestroy {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly applicationService = inject(InvestmentApplicationService);
  private readonly documentService = inject(InvestmentApplicationDocumentService);
  private readonly identityService = inject(InvestmentIdentityVerificationService);
  private readonly currencyFormatter = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });
  private readonly percentageFormatter = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  readonly application = signal<InvestmentApplication | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly cancelling = signal(false);
  readonly error = signal('');
  readonly documents = signal<InvestmentApplicationDocument[]>([]);
  readonly requirements = signal<InvestmentDocumentRequirements | null>(null);
  readonly uploadingType = signal<InvestmentDocumentType | null>(null);
  readonly completingDocuments = signal(false);
  readonly identityVerification = signal<InvestmentIdentityVerification | null>(null);
  readonly cameraActive = signal(false);
  readonly localPhotoUrl = signal<string | null>(null);
  readonly selfieUrl = signal<string | null>(null);
  readonly savingSelfie = signal(false);
  readonly verifyingIdentity = signal(false);
  readonly consentAccepted = signal(false);
  readonly reviewAccepted = signal(false);
  readonly submittingApplication = signal(false);
  private cameraStream: MediaStream | null = null;
  readonly form = this.formBuilder.group({
    firstName: ['', Validators.required], lastName: ['', Validators.required], identificationType: ['NationalId' as 'NationalId' | 'Passport', Validators.required],
    identificationNumber: ['', Validators.required], email: ['', [Validators.required, Validators.email]], phone: ['', Validators.required],
    birthDate: [''], address: ['', Validators.required], city: ['', Validators.required]
  });
  readonly steps: { key: InvestmentApplicationStep; label: string }[] = [
    { key: 'Investment', label: 'Inversión' }, { key: 'PersonalInformation', label: 'Datos personales' }, { key: 'Documents', label: 'Documentos' },
    { key: 'IdentityVerification', label: 'Validación de identidad' }, { key: 'Review', label: 'Revisión' }, { key: 'Confirmation', label: 'Confirmación' }
  ];

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.error.set('La solicitud no existe.'); this.loading.set(false); return; }
    this.applicationService.getById(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: application => { this.application.set(application); this.fillForm(application); this.loading.set(false); if (application.currentStep === 'Documents' || application.currentStep === 'Review' || application.currentStep === 'Confirmation') this.loadDocuments(application.id); if (application.currentStep === 'IdentityVerification' || application.currentStep === 'Review' || application.currentStep === 'Confirmation') this.loadIdentity(application.id); },
      error: error => { this.error.set(this.errorMessage(error)); this.loading.set(false); }
    });
  }

  saveApplicant(): void {
    const application = this.application();
    if (!application || this.isReadOnly() || this.saving()) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.error.set('');
    const value = this.form.getRawValue();
    const request: UpdateInvestmentApplicant = { ...value, birthDate: value.birthDate || null };
    this.applicationService.updateApplicant(application.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => { this.application.set(updated); this.saving.set(false); this.loadDocuments(updated.id); },
      error: error => { this.saving.set(false); this.error.set(this.errorMessage(error)); }
    });
  }

  cancel(): void {
    const application = this.application();
    if (!application || this.isReadOnly() || this.cancelling() || !confirm('¿Deseas cancelar esta solicitud de inversión?')) return;
    this.cancelling.set(true); this.error.set('');
    this.applicationService.cancel(application.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.application.update(current => current ? { ...current, status: 'Cancelled' } : current); this.cancelling.set(false); },
      error: error => { this.error.set(this.errorMessage(error)); this.cancelling.set(false); }
    });
  }

  goToSimulator(): void { this.router.navigate(['/client/investments/simulator']); }
  ngOnDestroy(): void { this.stopCamera(); this.releaseUrl(this.localPhotoUrl()); this.releaseUrl(this.selfieUrl()); }
  loadDocuments(applicationId: string): void {
    this.documentService.getAll(applicationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: documents => this.documents.set(documents), error: error => this.error.set(this.errorMessage(error)) });
    this.documentService.getRequirements(applicationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: requirements => this.requirements.set(requirements), error: error => this.error.set(this.errorMessage(error)) });
  }
  documentFor(type: InvestmentDocumentType): InvestmentApplicationDocument | null { return this.documents().find(document => document.documentType === type) ?? null; }
  additionalDocuments(): InvestmentApplicationDocument[] { return this.documents().filter(document => document.documentType === 'AdditionalDocument'); }
  requiredDocumentTypes(): InvestmentDocumentType[] { return this.requirements()?.requiredTypes ?? []; }
  chooseFile(event: Event, type: InvestmentDocumentType): void {
    const application = this.application(); const file = (event.target as HTMLInputElement).files?.[0];
    if (!application || !file || this.isReadOnly() || this.uploadingType()) return;
    const allowed = ['image/jpeg', 'image/png', 'application/pdf'];
    if (!allowed.includes(file.type)) { this.error.set('Solo se permiten archivos JPG, PNG o PDF.'); return; }
    if (file.size > 5 * 1024 * 1024) { this.error.set('El archivo no puede superar 5 MB.'); return; }
    this.uploadingType.set(type); this.error.set('');
    this.documentService.upload(application.id, type, file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.uploadingType.set(null); this.loadDocuments(application.id); },
      error: error => { this.uploadingType.set(null); this.error.set(this.documentErrorMessage(error)); }
    });
  }
  deleteDocument(document: InvestmentApplicationDocument): void {
    const application = this.application();
    if (!application || this.isReadOnly() || !confirm('¿Deseas eliminar este documento?')) return;
    this.documentService.delete(application.id, document.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: () => this.loadDocuments(application.id), error: error => this.error.set(this.documentErrorMessage(error)) });
  }
  downloadDocument(fileDocument: InvestmentApplicationDocument): void {
    const application = this.application(); if (!application) return;
    this.documentService.download(application.id, fileDocument.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: content => { const url = URL.createObjectURL(content); const anchor = globalThis.document.createElement('a'); anchor.href = url; anchor.download = fileDocument.originalFileName; anchor.click(); URL.revokeObjectURL(url); },
      error: error => this.error.set(this.documentErrorMessage(error))
    });
  }
  completeDocuments(): void {
    const application = this.application(); if (!application || !this.requirements()?.isComplete || this.completingDocuments() || this.isReadOnly()) return;
    this.completingDocuments.set(true); this.documentService.complete(application.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.applicationService.getById(application.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: updated => { this.application.set(updated); this.completingDocuments.set(false); }, error: error => { this.completingDocuments.set(false); this.error.set(this.errorMessage(error)); } }),
      error: error => { this.completingDocuments.set(false); this.error.set(this.documentErrorMessage(error)); }
    });
  }
  documentLabel(type: InvestmentDocumentType): string { return ({ IdentityFront: 'Documento de identidad - frontal', IdentityBack: 'Documento de identidad - reverso', AdditionalDocument: 'Documento adicional' } as Record<InvestmentDocumentType, string>)[type]; }
  fileSize(size: number): string { return `${(size / 1024 / 1024).toLocaleString('es-EC', { maximumFractionDigits: 2 })} MB`; }
  async startCamera(): Promise<void> {
    if (this.isReadOnly() || !navigator.mediaDevices?.getUserMedia) { this.error.set('No se detectó una cámara disponible.'); return; }
    try { this.cameraStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' } }); this.cameraActive.set(true); setTimeout(() => { const video = globalThis.document.querySelector<HTMLVideoElement>('#identityCamera'); if (video && this.cameraStream) { video.srcObject = this.cameraStream; video.play(); } }); }
    catch { this.error.set('No se pudo acceder a la cámara. Puedes seleccionar una fotografía desde tu dispositivo.'); }
  }
  stopCamera(): void { this.cameraStream?.getTracks().forEach(track => track.stop()); this.cameraStream = null; this.cameraActive.set(false); }
  captureSelfie(): void { const video = globalThis.document.querySelector<HTMLVideoElement>('#identityCamera'); if (!video) return; const canvas = globalThis.document.createElement('canvas'); canvas.width = video.videoWidth; canvas.height = video.videoHeight; canvas.getContext('2d')?.drawImage(video, 0, 0); canvas.toBlob(blob => { if (!blob) return; this.setLocalPhoto(new File([blob], 'selfie.jpg', { type: 'image/jpeg' })); }, 'image/jpeg', .9); this.stopCamera(); }
  selectSelfie(event: Event): void { const file = (event.target as HTMLInputElement).files?.[0]; if (!file) return; if (!['image/jpeg','image/png'].includes(file.type)) { this.error.set('Solo se permiten imágenes JPG o PNG.'); return; } if (file.size > 5 * 1024 * 1024) { this.error.set('La imagen no puede superar 5 MB.'); return; } this.setLocalPhoto(file); }
  discardLocalPhoto(): void { this.releaseUrl(this.localPhotoUrl()); this.localPhotoUrl.set(null); }
  saveSelfie(): void { const app = this.application(); const url = this.localPhotoUrl(); if (!app || !url || this.savingSelfie()) return; const file = this.pendingSelfie; if (!file) return; this.savingSelfie.set(true); this.identityService.uploadSelfie(app.id,file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:v=>{this.identityVerification.set(v);this.selfieUrl.set(url);this.localPhotoUrl.set(null);this.pendingSelfie=null;this.savingSelfie.set(false);},error:e=>{this.savingSelfie.set(false);this.error.set(this.identityError(e));}}); }
  verifyIdentity(): void { const app=this.application();if(!app||!this.consentAccepted()||this.verifyingIdentity())return;if(!this.identityVerification()||this.identityVerification()?.status!=='Captured'){this.error.set('Primero debes registrar una fotografía.');return;}this.verifyingIdentity.set(true);this.identityService.verify(app.id,true).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:v=>{this.identityVerification.set(v);this.applicationService.getById(app.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:x=>{this.application.set(x);this.verifyingIdentity.set(false);},error:e=>{this.verifyingIdentity.set(false);this.error.set(this.errorMessage(e));}})},error:e=>{this.verifyingIdentity.set(false);this.error.set(this.identityError(e));}}); }
  submitApplication(): void { const app=this.application();if(!app||!this.reviewAccepted()||this.submittingApplication())return;this.submittingApplication.set(true);this.applicationService.submit(app.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:updated=>{this.application.set(updated);this.submittingApplication.set(false);},error:error=>{this.submittingApplication.set(false);this.error.set(this.submitError(error));}}); }
  private pendingSelfie: File | null = null;
  private setLocalPhoto(file: File): void { this.releaseUrl(this.localPhotoUrl()); this.pendingSelfie=file; this.localPhotoUrl.set(URL.createObjectURL(file)); }
  private loadIdentity(id: string): void { this.identityService.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:v=>{this.identityVerification.set(v);this.consentAccepted.set(v.consentAccepted);if(v.status==='Captured'||v.status==='Verified')this.identityService.getSelfie(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:b=>this.selfieUrl.set(URL.createObjectURL(b))});},error:e=>{if(e.status!==404)this.error.set(this.identityError(e));}}); }
  private releaseUrl(url:string|null):void{if(url)URL.revokeObjectURL(url);}
  money(value: number): string { return this.currencyFormatter.format(value); }
  percent(value: number): string { return `${this.percentageFormatter.format(value)} %`; }
  methodLabel(method: string): string { return method === 'Compound' ? 'Interés compuesto' : 'Interés simple'; }
  frequencyLabel(frequency: string): string { return ({ AtMaturity: 'Al vencimiento', Monthly: 'Mensual', Quarterly: 'Trimestral', SemiAnnual: 'Semestral', Upfront: 'Anticipado' } as Record<string, string>)[frequency] ?? frequency; }
  statusLabel(status: InvestmentApplicationStatus): string { return ({ Draft: 'Borrador', PendingDocuments: 'Pendiente de documentos', PendingIdentityVerification: 'Pendiente de identidad', ReadyForReview: 'Lista para enviar', Submitted: 'En revisión', Approved: 'Aprobada', Rejected: 'Rechazada', Cancelled: 'Cancelada' } as Record<string, string>)[status]; }
  isReadOnly(): boolean { return this.application()?.status === 'Cancelled'; }
  isComplete(step: InvestmentApplicationStep): boolean { const current = this.application()?.currentStep ?? 'PersonalInformation'; return this.stepIndex(step) < this.stepIndex(current); }
  isCurrent(step: InvestmentApplicationStep): boolean { return this.application()?.currentStep === step; }
  private stepIndex(step: InvestmentApplicationStep): number { return this.steps.findIndex(item => item.key === step); }
  private fillForm(application: InvestmentApplication): void {
    this.form.patchValue({ firstName: application.applicantFirstName ?? '', lastName: application.applicantLastName ?? '', identificationType: application.identificationType ?? 'NationalId', identificationNumber: application.identificationNumber ?? '', email: application.email ?? '', phone: application.phone ?? '', birthDate: application.birthDate ?? '', address: application.address ?? '', city: application.city ?? '' });
  }
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 403) return 'No tienes acceso a esta solicitud.';
    if (error.status === 404) return 'La solicitud no existe.';
    if (error.status === 400) return 'Revisa la información ingresada.';
    return 'Ocurrió un error al procesar la solicitud.';
  }
  private documentErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 403) return 'No tienes acceso a esta solicitud.';
    if (error.status === 404) return 'No se encontró el documento o la solicitud.';
    if (error.status === 413) return 'El archivo supera el tamaño permitido.';
    if (error.status === 415) return 'El tipo de archivo no está permitido.';
    if (error.status === 400) return 'El documento no cumple los requisitos.';
    return 'No fue posible procesar el documento.';
  }
  private identityError(error: HttpErrorResponse): string { if(error.status===0)return 'No se pudo conectar con el servidor.';if(error.status===403)return 'No tienes acceso a esta solicitud.';if(error.status===413)return 'La imagen no puede superar 5 MB.';if(error.status===415)return 'Solo se permiten imágenes JPG o PNG.';if(error.status===404)return 'No se encontró la solicitud.';return 'No fue posible procesar la fotografía.'; }
  private submitError(error: HttpErrorResponse): string { if(error.status===0)return 'No se pudo conectar con el servidor.';if(error.status===403)return 'No tienes acceso a esta solicitud.';if(error.status===400)return 'La solicitud aún no cumple todos los requisitos para ser enviada.';if(error.status===404)return 'La solicitud no existe.';return 'No fue posible enviar la solicitud.'; }
}
