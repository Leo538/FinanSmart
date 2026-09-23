import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, OnDestroy, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { InvestmentApplication, InvestmentApplicationStatus, InvestmentApplicationStep, SourceOfFunds, UpdateInvestmentApplicant, UpdateInvestmentDeclarations } from './models/investment-application.model';
import { InvestmentApplicationService } from './services/investment-application.service';
import { InvestmentApplicationDocument, InvestmentDocumentRequirements, InvestmentDocumentType } from './models/investment-application-document.model';
import { InvestmentApplicationDocumentService } from './services/investment-application-document.service';
import { InvestmentIdentityVerification } from './models/investment-identity-verification.model';
import { InvestmentIdentityVerificationService } from './services/investment-identity-verification.service';

interface DetectedFace { boundingBox: DOMRectReadOnly; }
interface FaceDetectorLike { detect(source: ImageBitmapSource): Promise<DetectedFace[]>; }
interface FaceDetectorConstructor { new(options?: { fastMode?: boolean; maxDetectedFaces?: number }): FaceDetectorLike; }

const isValidEcuadorNationalId = (value: string): boolean => {
  if (!/^\d{10}$/.test(value)) return false;
  const digits = [...value].map(Number); const province = digits[0] * 10 + digits[1];
  if (province < 1 || province > 24 || digits[2] > 5) return false;
  const sum = digits.slice(0, 9).reduce((total, digit, index) => { const weighted = digit * (index % 2 === 0 ? 2 : 1); return total + (weighted > 9 ? weighted - 9 : weighted); }, 0);
  return (10 - sum % 10) % 10 === digits[9];
};

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
  readonly uploadingFileName = signal('');
  readonly completingDocuments = signal(false);
  readonly documentCameraActive = signal(false);
  readonly documentCaptureType = signal<InvestmentDocumentType | null>(null);
  readonly documentGuideMessage = signal('Coloca toda la cédula dentro del marco.');
  readonly documentReady = signal(false);
  readonly documentProgress = signal(0);
  readonly documentPreviewUrl = signal<string | null>(null);
  readonly identityVerification = signal<InvestmentIdentityVerification | null>(null);
  readonly cameraActive = signal(false);
  readonly faceDetectionAvailable = signal(false);
  readonly faceGuideMessage = signal('Coloca tu rostro dentro del marco.');
  readonly faceReady = signal(false);
  readonly faceStabilityProgress = signal(0);
  readonly localPhotoUrl = signal<string | null>(null);
  readonly selfieValidated = signal(false);
  readonly selfieUrl = signal<string | null>(null);
  readonly savingSelfie = signal(false);
  readonly verifyingIdentity = signal(false);
  readonly consentAccepted = signal(false);
  readonly reviewAccepted = signal(false);
  readonly submittingApplication = signal(false);
  readonly savingDeclarations = signal(false);
  private cameraStream: MediaStream | null = null;
  private faceDetector: FaceDetectorLike | null = null;
  private detectionTimer: ReturnType<typeof setTimeout> | null = null;
  private detectionBusy = false;
  private stableSince: number | null = null;
  private lastFaceCenter: { x: number; y: number } | null = null;
  private documentStream: MediaStream | null = null;
  private documentTimer: ReturnType<typeof setTimeout> | null = null;
  private documentStableSince: number | null = null;
  readonly form = this.formBuilder.group({
    firstName: ['', Validators.required], lastName: ['', Validators.required], identificationType: ['NationalId' as 'NationalId' | 'Passport', Validators.required],
    identificationNumber: ['', Validators.required], email: ['', [Validators.required, Validators.email]], phone: ['', Validators.required],
    birthDate: [''], address: ['', Validators.required], city: ['', Validators.required]
  });
  readonly declarationsForm = this.formBuilder.group({
    sourceOfFunds: [null as SourceOfFunds | null, Validators.required],
    otherSourceOfFunds: [''],
    informationAccuracyAccepted: [false, Validators.requiredTrue],
    termsAccepted: [false, Validators.requiredTrue],
    dataProcessingAccepted: [false, Validators.requiredTrue]
  });
  readonly sourceOfFundsOptions: { value: SourceOfFunds; label: string }[] = [
    { value: 'Salary', label: 'Sueldo' }, { value: 'Savings', label: 'Ahorros' },
    { value: 'BusinessActivity', label: 'Actividad comercial' }, { value: 'Other', label: 'Otros' }
  ];
  readonly steps: { key: InvestmentApplicationStep; label: string }[] = [
    { key: 'Investment', label: 'Inversión' }, { key: 'PersonalInformation', label: 'Datos personales' }, { key: 'Documents', label: 'Documentos' },
    { key: 'IdentityVerification', label: 'Validación de identidad' }, { key: 'Declarations', label: 'Declaraciones' }, { key: 'Review', label: 'Revisión' }, { key: 'Confirmation', label: 'Confirmación' }
  ];

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.error.set('La solicitud no existe.'); this.loading.set(false); return; }
    this.applicationService.getById(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: application => { this.application.set(application); this.fillForm(application); this.loading.set(false); if (['Documents', 'IdentityVerification', 'Declarations', 'Review', 'Confirmation'].includes(application.currentStep)) this.loadDocuments(application.id); if (['IdentityVerification', 'Declarations', 'Review', 'Confirmation'].includes(application.currentStep)) this.loadIdentity(application.id); },
      error: error => { this.error.set(this.errorMessage(error)); this.loading.set(false); }
    });
  }

  saveApplicant(): void {
    const application = this.application();
    if (!application || this.isReadOnly() || this.saving()) return;
    if (this.form.invalid || !this.applicantIdentityValid() || !this.applicantIsAdult()) { this.form.markAllAsTouched(); return; }
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
  applicantIdentityValid(): boolean { const value = this.form.getRawValue(); return value.identificationType !== 'NationalId' || isValidEcuadorNationalId(value.identificationNumber); }
  applicantIsAdult(): boolean { const birthDate = this.form.controls.birthDate.value; return !birthDate || new Date(`${birthDate}T00:00:00`).getTime() <= new Date(new Date().setFullYear(new Date().getFullYear() - 18)).getTime(); }
  identificationError(): string { return this.form.controls.identificationType.value === 'NationalId' && !this.applicantIdentityValid() ? 'Número de cédula ecuatoriana no válido.' : ''; }
  birthDateError(): string { return !this.applicantIsAdult() ? 'Debes ser mayor de edad para continuar.' : ''; }
  ngOnDestroy(): void { this.stopCamera(); this.stopDocumentCamera(); this.releaseUrl(this.localPhotoUrl()); this.releaseUrl(this.selfieUrl()); this.releaseUrl(this.documentPreviewUrl()); }
  startDocumentCamera(type: InvestmentDocumentType): void { this.documentCaptureType.set(type); this.documentGuideMessage.set('Coloca toda la cédula dentro del marco.'); this.documentReady.set(false); this.documentProgress.set(0); navigator.mediaDevices?.getUserMedia({ video: { facingMode: { ideal: 'environment' } }, audio: false }).then(stream => { this.documentStream = stream; this.documentCameraActive.set(true); setTimeout(async () => { const video = globalThis.document.querySelector<HTMLVideoElement>('#documentCamera'); if (video) { video.srcObject = stream; await video.play(); this.detectDocument(video); } }); }).catch(() => this.error.set('No se pudo acceder a la cámara. Puedes subir un archivo.')); }
  stopDocumentCamera(): void { if (this.documentTimer) clearTimeout(this.documentTimer); this.documentTimer=null; this.documentStableSince=null; this.documentStream?.getTracks().forEach(track=>track.stop()); this.documentStream=null; this.documentCameraActive.set(false); }
  private detectDocument(video: HTMLVideoElement): void { if (!this.documentCameraActive()) return; this.documentTimer=setTimeout(async()=>{ if (!video.videoWidth) return this.detectDocument(video); const canvas=globalThis.document.createElement('canvas'); canvas.width=120; canvas.height=75; const context=canvas.getContext('2d',{willReadFrequently:true}); if (!context) return; context.drawImage(video,0,0,120,75); const analysis=this.documentAnalysis(context.getImageData(0,0,120,75)); if (!analysis.valid) { this.documentGuideMessage.set(analysis.message); this.resetDocumentGuide(); if(this.documentCaptureType()==='IdentityFront') await this.checkLargeFace(video,analysis.hasRectangle); } else { const now=performance.now();this.documentStableSince??=now;const progress=Math.min(1,(now-this.documentStableSince)/1000);this.documentProgress.set(progress);this.documentReady.set(true);this.documentGuideMessage.set(progress>=1?'Documento correctamente posicionado.':'Mantén el documento recto.');if(progress>=1)this.captureDocument(video); } if(this.documentCameraActive())this.detectDocument(video);},120); }
  private documentAnalysis(image:ImageData):{valid:boolean;hasRectangle:boolean;message:string}{const w=image.width,h=image.height,data=image.data;const edges:boolean[]=[];let minX=w,minY=h,maxX=0,maxY=0,count=0;for(let y=1;y<h-1;y++)for(let x=1;x<w-1;x++){const p=(y*w+x)*4,l=.2126*data[p]+.7152*data[p+1]+.0722*data[p+2],r=.2126*data[p+4]+.7152*data[p+5]+.0722*data[p+6],d=.2126*data[p+w*4]+.7152*data[p+w*4+1]+.0722*data[p+w*4+2];if(Math.abs(l-r)+Math.abs(l-d)>65){edges[y*w+x]=true;count++;minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);}}const bw=maxX-minX,bh=maxY-minY,area=bw*bh/(w*h),ratio=bh?bw/bh:0,center=Math.abs((minX+maxX)/2/w-.5)<.15&&Math.abs((minY+maxY)/2/h-.5)<.16;const rectangle=count>90&&area>.22&&area<.78&&ratio>1.25&&ratio<2.05; if(!count)return{valid:false,hasRectangle:false,message:'No se detecta correctamente un documento de identidad.'};if(area<=.22)return{valid:false,hasRectangle:false,message:'Acerca el documento.'};if(area>=.78)return{valid:false,hasRectangle:false,message:'Aleja el documento y muestra las cuatro esquinas.'};if(!center)return{valid:false,hasRectangle:rectangle,message:'Centra la cédula dentro del marco.'};if(!rectangle)return{valid:false,hasRectangle:false,message:'No se detecta correctamente un documento de identidad.'};return{valid:true,hasRectangle:true,message:''};}
  private async checkLargeFace(video:HTMLVideoElement,hasRectangle:boolean):Promise<void>{const Detector=(globalThis as typeof globalThis&{FaceDetector?:FaceDetectorConstructor}).FaceDetector;if(!Detector||hasRectangle)return;try{const faces=await new Detector({fastMode:true,maxDetectedFaces:1}).detect(video);if(faces[0]&&faces[0].boundingBox.width/video.videoWidth>.42)this.documentGuideMessage.set('La imagen parece una fotografía personal y no un documento completo.');}catch{}}
  private resetDocumentGuide(): void { this.documentStableSince=null;this.documentReady.set(false);this.documentProgress.set(0); }
  private captureDocument(video:HTMLVideoElement):void { const type=this.documentCaptureType();if(!type)return; const canvas=globalThis.document.createElement('canvas');canvas.width=video.videoWidth;canvas.height=video.videoHeight;canvas.getContext('2d')?.drawImage(video,0,0);canvas.toBlob(blob=>{if(!blob)return;this.releaseUrl(this.documentPreviewUrl());this.documentPreviewUrl.set(URL.createObjectURL(blob));this.stopDocumentCamera();},'image/jpeg',.92); }
  repeatDocumentPhoto():void{const type=this.documentCaptureType();this.releaseUrl(this.documentPreviewUrl());this.documentPreviewUrl.set(null);if(type)this.startDocumentCamera(type);}
  useDocumentPhoto():void{const type=this.documentCaptureType(),url=this.documentPreviewUrl(),app=this.application();if(!type||!url||!app)return;fetch(url).then(x=>x.blob()).then(blob=>{const file=new File([blob],`${type}.jpg`,{type:'image/jpeg'});this.documentService.upload(app.id,type,file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:()=>{this.releaseUrl(url);this.documentPreviewUrl.set(null);this.documentCaptureType.set(null);this.loadDocuments(app.id);},error:e=>this.error.set(this.documentErrorMessage(e))});});}
  loadDocuments(applicationId: string): void {
    this.documentService.getAll(applicationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: documents => this.documents.set(documents), error: error => this.error.set(this.errorMessage(error)) });
    this.documentService.getRequirements(applicationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: requirements => this.requirements.set(requirements), error: error => this.error.set(this.errorMessage(error)) });
  }
  documentFor(type: InvestmentDocumentType): InvestmentApplicationDocument | null { return this.documents().find(document => document.documentType === type) ?? null; }
  additionalDocuments(): InvestmentApplicationDocument[] { return this.documents().filter(document => document.documentType === 'AdditionalDocument'); }
  requiredDocumentTypes(): InvestmentDocumentType[] { return this.requirements()?.requiredTypes ?? []; }
  async chooseFile(event: Event, type: InvestmentDocumentType): Promise<void> {
    const application = this.application(); const file = (event.target as HTMLInputElement).files?.[0];
    if (!application || !file || this.isReadOnly() || this.uploadingType()) return;
    const allowed = type === 'AdditionalDocument' ? ['image/jpeg', 'image/png', 'application/pdf'] : ['image/jpeg', 'image/png'];
    if (!allowed.includes(file.type)) { this.error.set('Solo se permiten archivos JPG, PNG o PDF.'); return; }
    if (file.size > 5 * 1024 * 1024) { this.error.set('El archivo no puede superar 5 MB.'); return; }
    if (file.type !== 'application/pdf' && !await this.validateDocumentImageFile(file, type)) return;
    this.uploadingType.set(type); this.uploadingFileName.set(file.name); this.error.set('');
    this.documentService.upload(application.id, type, file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.uploadingType.set(null); this.uploadingFileName.set(''); this.loadDocuments(application.id); },
      error: error => { this.uploadingType.set(null); this.uploadingFileName.set(''); this.error.set(this.documentErrorMessage(error)); }
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
  documentLabel(type: InvestmentDocumentType): string { return ({ IdentityFront: 'Cédula frontal', IdentityBack: 'Cédula reverso', AdditionalDocument: 'Comprobante de domicilio u otro documento de respaldo (opcional)' } as Record<InvestmentDocumentType, string>)[type]; }
  documentValidationLabel(document: InvestmentApplicationDocument): string { return ({ Valid: 'Documento validado', Invalid: 'No pudimos validar el documento', RequiresManualReview: 'Documento requiere revisión', Pending: 'Pendiente de análisis', Analyzing: 'Analizando documento...' } as Record<string, string>)[document.validationStatus ?? 'Pending']; }
  maskDocumentNumber(value: string | null): string { return value ? `••••••${value.slice(-4)}` : ''; }
  fileSize(size: number): string { return `${(size / 1024 / 1024).toLocaleString('es-EC', { maximumFractionDigits: 2 })} MB`; }
  sourceOfFundsLabel(source: SourceOfFunds | null): string { return this.sourceOfFundsOptions.find(option => option.value === source)?.label ?? 'No registrado'; }
  async startCamera(): Promise<void> {
    if (this.isReadOnly() || !navigator.mediaDevices?.getUserMedia) { this.error.set('No se detectó una cámara disponible. Puedes seleccionar una fotografía.'); return; }
    try {
      this.cameraStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false });
      this.cameraActive.set(true); this.resetFaceGuide();
      setTimeout(async () => {
        const video = globalThis.document.querySelector<HTMLVideoElement>('#identityCamera');
        if (!video || !this.cameraStream) return;
        video.srcObject = this.cameraStream; await video.play();
        const FaceDetector = (globalThis as typeof globalThis & { FaceDetector?: FaceDetectorConstructor }).FaceDetector;
        if (!FaceDetector) { this.faceDetectionAvailable.set(false); this.faceGuideMessage.set('La detección automática no está disponible; captura una selfie en vivo con la cámara.'); return; }
        this.faceDetector = new FaceDetector({ fastMode: true, maxDetectedFaces: 2 }); this.faceDetectionAvailable.set(true); this.detectFace(video);
      });
    } catch { this.error.set('No se pudo acceder a la cámara. Puedes seleccionar una fotografía desde tu dispositivo.'); }
  }
  stopCamera(): void { if (this.detectionTimer) clearTimeout(this.detectionTimer); this.detectionTimer = null; this.faceDetector = null; this.detectionBusy = false; this.stableSince = null; this.lastFaceCenter = null; this.cameraStream?.getTracks().forEach(track => track.stop()); this.cameraStream = null; this.cameraActive.set(false); }
  repeatPhoto(): void { this.discardLocalPhoto(); this.startCamera(); }
  captureSelfie(): void { const video = globalThis.document.querySelector<HTMLVideoElement>('#identityCamera'); if (!video || !video.videoWidth) { this.error.set('No se pudo leer la cámara. Intenta activarla nuevamente.'); return; } if (this.faceDetector && !this.faceReady()) { this.error.set('Espera a que se confirme un rostro correctamente posicionado.'); return; } const canvas = globalThis.document.createElement('canvas'); canvas.width = video.videoWidth; canvas.height = video.videoHeight; canvas.getContext('2d')?.drawImage(video, 0, 0); canvas.toBlob(blob => { if (!blob) return; this.selfieValidated.set(true); this.setLocalPhoto(new File([blob], 'selfie.jpg', { type: 'image/jpeg' })); }, 'image/jpeg', .9); this.stopCamera(); }
  async selectSelfie(event: Event): Promise<void> { const file = (event.target as HTMLInputElement).files?.[0]; if (!file) return; if (!await this.validateSelfieFile(file)) return; this.stopCamera(); this.selfieValidated.set(true); this.setLocalPhoto(file); }
  discardLocalPhoto(): void { this.releaseUrl(this.localPhotoUrl()); this.localPhotoUrl.set(null); this.pendingSelfie = null; this.selfieValidated.set(false); }
  saveSelfie(): void { const app = this.application(); const url = this.localPhotoUrl(); if (!app || !url || this.savingSelfie() || !this.selfieValidated()) { this.error.set('La fotografía debe contener un solo rostro validado.'); return; } const file = this.pendingSelfie; if (!file) return; this.savingSelfie.set(true); this.identityService.uploadSelfie(app.id,file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:v=>{this.identityVerification.set(v);this.selfieUrl.set(url);this.localPhotoUrl.set(null);this.pendingSelfie=null;this.selfieValidated.set(false);this.savingSelfie.set(false);},error:e=>{this.savingSelfie.set(false);this.error.set(this.identityError(e));}}); }
  verifyIdentity(): void { const app=this.application();if(!app||!this.consentAccepted()||this.verifyingIdentity())return;if(!this.identityVerification()||this.identityVerification()?.status!=='Captured'){this.error.set('Primero debes registrar una fotografía.');return;}this.verifyingIdentity.set(true);this.identityService.verify(app.id,true).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:v=>{this.identityVerification.set(v);this.applicationService.getById(app.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:x=>{this.application.set(x);this.verifyingIdentity.set(false);},error:e=>{this.verifyingIdentity.set(false);this.error.set(this.errorMessage(e));}})},error:e=>{this.verifyingIdentity.set(false);this.error.set(this.identityError(e));}}); }
  declarationsReady(): boolean {
    const value = this.declarationsForm.getRawValue();
    return this.declarationsForm.valid && (value.sourceOfFunds !== 'Other' || !!value.otherSourceOfFunds.trim());
  }
  saveDeclarations(): void {
    const application = this.application();
    if (!application || this.savingDeclarations() || this.isReadOnly()) return;
    if (!this.declarationsReady()) { this.declarationsForm.markAllAsTouched(); return; }
    const value = this.declarationsForm.getRawValue();
    const request: UpdateInvestmentDeclarations = { ...value, otherSourceOfFunds: value.sourceOfFunds === 'Other' ? value.otherSourceOfFunds.trim() : null };
    this.savingDeclarations.set(true); this.error.set('');
    this.applicationService.updateDeclarations(application.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => { this.application.set(updated); this.savingDeclarations.set(false); },
      error: error => { this.savingDeclarations.set(false); this.error.set(this.declarationsError(error)); }
    });
  }
  submitApplication(): void { const app=this.application();if(!app||!this.reviewAccepted()||this.submittingApplication())return;this.submittingApplication.set(true);this.applicationService.submit(app.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:updated=>{this.application.set(updated);this.submittingApplication.set(false);},error:error=>{this.submittingApplication.set(false);this.error.set(this.submitError(error));}}); }
  private pendingSelfie: File | null = null;
  private setLocalPhoto(file: File): void { this.releaseUrl(this.localPhotoUrl()); this.pendingSelfie=file; this.localPhotoUrl.set(URL.createObjectURL(file)); }
  private resetFaceGuide(): void { this.faceGuideMessage.set('Coloca tu rostro dentro del marco.'); this.faceReady.set(false); this.faceStabilityProgress.set(0); this.stableSince = null; this.lastFaceCenter = null; }
  private detectFace(video: HTMLVideoElement): void {
    if (!this.cameraActive() || !this.faceDetector) return;
    this.detectionTimer = setTimeout(async () => {
      if (!this.cameraActive() || !this.faceDetector || this.detectionBusy) return;
      this.detectionBusy = true;
      try {
        if (video.readyState < HTMLMediaElement.HAVE_CURRENT_DATA) { this.resetFaceGuide(); }
        else { this.evaluateFaces(await this.faceDetector.detect(video), video); }
      } catch { this.faceGuideMessage.set('No se pudo detectar el rostro. Puedes usar la captura manual.'); this.faceDetectionAvailable.set(false); }
      finally { this.detectionBusy = false; if (this.cameraActive() && this.faceDetector) this.detectFace(video); }
    }, 120);
  }
  private evaluateFaces(faces: DetectedFace[], video: HTMLVideoElement): void {
    if (faces.length === 0) { this.resetFaceGuide(); this.faceGuideMessage.set('No se detecta un rostro.'); return; }
    if (faces.length !== 1) { this.resetFaceGuide(); this.faceGuideMessage.set('Debe aparecer únicamente una persona.'); return; }
    const box = faces[0].boundingBox; const widthRatio = box.width / video.videoWidth;
    const center = { x: (box.x + box.width / 2) / video.videoWidth, y: (box.y + box.height / 2) / video.videoHeight };
    if (widthRatio < .26) { this.resetFaceGuide(); this.faceGuideMessage.set('Acércate un poco.'); return; }
    if (widthRatio > .60) { this.resetFaceGuide(); this.faceGuideMessage.set('Aléjate un poco.'); return; }
    if (Math.abs(center.x - .5) > .14 || Math.abs(center.y - .48) > .18) { this.resetFaceGuide(); this.faceGuideMessage.set('Centra tu rostro dentro del marco.'); return; }
    const moved = this.lastFaceCenter && Math.hypot(center.x - this.lastFaceCenter.x, center.y - this.lastFaceCenter.y) > .025;
    this.lastFaceCenter = center;
    if (moved) { this.stableSince = null; this.faceStabilityProgress.set(0); this.faceGuideMessage.set('Mantén la posición...'); return; }
    const now = performance.now(); this.stableSince ??= now;
    const progress = Math.min(1, (now - this.stableSince) / 1200); this.faceStabilityProgress.set(progress); this.faceReady.set(true); this.faceGuideMessage.set(progress >= 1 ? 'Rostro correctamente posicionado.' : 'Mantén la posición...');
    if (progress >= 1) this.captureSelfie();
  }
  private async validateSelfieFile(file: File): Promise<boolean> {
    if (!await this.validateImageFile(file)) return false;
    const Detector = (globalThis as typeof globalThis & { FaceDetector?: FaceDetectorConstructor }).FaceDetector;
    if (!Detector) { this.error.set(''); await this.startCamera(); return false; }
    let bitmap: ImageBitmap | null = null;
    try {
      bitmap = await createImageBitmap(file);
      const faces = await new Detector({ fastMode: false, maxDetectedFaces: 2 }).detect(bitmap);
      if (faces.length !== 1) { this.error.set(faces.length ? 'La fotografía debe contener únicamente un rostro.' : 'No se detectó un rostro. No subas una cédula ni otro documento.'); return false; }
      const faceRatio = faces[0].boundingBox.width / bitmap.width;
      if (faceRatio < .22 || faceRatio > .68) { this.error.set('El rostro debe verse centrado y ocupar una parte clara de la fotografía.'); return false; }
      return true;
    } catch { this.error.set('No se pudo validar el rostro en la fotografía.'); return false; }
    finally { bitmap?.close(); }
  }
  private async validateImageFile(file: File): Promise<boolean> {
    if (!['image/jpeg', 'image/png'].includes(file.type)) { this.error.set('Solo se permiten imágenes JPG o PNG.'); return false; }
    if (!file.size) { this.error.set('La imagen está vacía.'); return false; }
    if (file.size > 5 * 1024 * 1024) { this.error.set('La imagen no puede superar 5 MB.'); return false; }
    const url = URL.createObjectURL(file);
    try { await new Promise<void>((resolve, reject) => { const image = new Image(); image.onload = () => resolve(); image.onerror = () => reject(); image.src = url; }); return true; }
    catch { this.error.set('No fue posible leer la imagen seleccionada.'); return false; }
    finally { URL.revokeObjectURL(url); }
  }
  private async validateDocumentImageFile(file: File, type: InvestmentDocumentType): Promise<boolean> {
    if (!await this.validateImageFile(file)) return false;
    const url = URL.createObjectURL(file);
    try {
      const image = await new Promise<HTMLImageElement>((resolve, reject) => { const element = new Image(); element.onload = () => resolve(element); element.onerror = () => reject(); element.src = url; });
      if (image.naturalWidth < 640 || image.naturalHeight < 400) { this.error.set('La imagen debe tener al menos 640 × 400 píxeles para revisión.'); return false; }
      const ratio = image.naturalWidth / image.naturalHeight;
      if (ratio < .65 || ratio > 2.8) { this.error.set('La proporción de la imagen no es adecuada para revisión.'); return false; }
      const canvas = globalThis.document.createElement('canvas'); canvas.width = 32; canvas.height = 32;
      const context = canvas.getContext('2d', { willReadFrequently: true }); if (!context) return false;
      context.drawImage(image, 0, 0, canvas.width, canvas.height);
      const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
      let luminance = 0; for (let index = 0; index < pixels.length; index += 4) luminance += .2126 * pixels[index] + .7152 * pixels[index + 1] + .0722 * pixels[index + 2];
      const average = luminance / (pixels.length / 4);
      if (average < 18 || average > 242) { this.error.set('La imagen está demasiado oscura o clara para revisión.'); return false; }
      if (type !== 'AdditionalDocument' && (ratio < 1.1 || ratio > 2.2)) { this.error.set('La imagen no parece corresponder a un documento de identidad completo.'); return false; }
      return true;
    } catch { this.error.set('No fue posible analizar la imagen del documento.'); return false; }
    finally { URL.revokeObjectURL(url); }
  }
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
    this.declarationsForm.patchValue({ sourceOfFunds: application.sourceOfFunds, otherSourceOfFunds: application.otherSourceOfFunds ?? '', informationAccuracyAccepted: application.informationAccuracyAccepted, termsAccepted: application.termsAccepted, dataProcessingAccepted: application.dataProcessingAccepted });
  }
  private errorMessage(error: HttpErrorResponse): string {
    if (typeof error.error?.message === 'string') return error.error.message;
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 403) return 'No tienes acceso a esta solicitud.';
    if (error.status === 404) return 'La solicitud no existe.';
    if (error.status === 400) return 'Revisa la información ingresada.';
    return 'Ocurrió un error al procesar la solicitud.';
  }
  private documentErrorMessage(error: HttpErrorResponse): string {
    if (typeof error.error?.message === 'string') return error.error.message;
    if (error.status === 0) return 'No se pudo conectar con el servidor.';
    if (error.status === 403) return 'No tienes acceso a esta solicitud.';
    if (error.status === 404) return 'No se encontró el documento o la solicitud.';
    if (error.status === 413) return 'El archivo supera el tamaño permitido.';
    if (error.status === 415) return 'El tipo de archivo no está permitido.';
    if (error.status === 400) return 'El documento no cumple los requisitos.';
    return 'No fue posible procesar el documento.';
  }
  private identityError(error: HttpErrorResponse): string { if(error.status===0)return 'No se pudo conectar con el servidor.';if(error.status===403)return 'No tienes acceso a esta solicitud.';if(error.status===413)return 'La imagen no puede superar 5 MB.';if(error.status===415)return 'Solo se permiten imágenes JPG o PNG.';if(error.status===404)return 'No se encontró la solicitud.';return 'No fue posible procesar la fotografía.'; }
  private declarationsError(error: HttpErrorResponse): string { if(error.status===0)return 'No se pudo conectar con el servidor.';if(error.status===403)return 'No tienes acceso a esta solicitud.';if(error.status===404)return 'La solicitud no existe.';if(error.status===400)return 'Revisa las declaraciones antes de continuar.';return 'No fue posible guardar las declaraciones.'; }
  private submitError(error: HttpErrorResponse): string { if(error.status===0)return 'No se pudo conectar con el servidor.';if(error.status===403)return 'No tienes acceso a esta solicitud.';if(error.status===400)return 'La solicitud aún no cumple todos los requisitos para ser enviada.';if(error.status===404)return 'La solicitud no existe.';return 'No fue posible enviar la solicitud.'; }
}
