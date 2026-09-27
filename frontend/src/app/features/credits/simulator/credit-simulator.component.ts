import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { CreditType } from '../../admin/credits/credit-types/models/credit-type.model';
import { CreditTypeService } from '../../admin/credits/credit-types/services/credit-type.service';
import { CreditRateService } from '../../admin/credits/credit-rates/services/credit-rate.service';
import { AmortizationInstallment, AmortizationSystem, CreditSimulationRequest, CreditSimulationResponse } from './models/credit-simulation.model';
import { CreditSimulationService } from './services/credit-simulation.service';

@Component({selector:'app-credit-simulator',imports:[ReactiveFormsModule,DatePipe,PageHeaderComponent,LoadingSpinnerComponent],templateUrl:'./credit-simulator.component.html',styleUrl:'./credit-simulator.component.scss'})
export class CreditSimulatorComponent {
  private readonly destroy=inject(DestroyRef); private readonly fb=inject(NonNullableFormBuilder); private readonly typeApi=inject(CreditTypeService); private readonly rateApi=inject(CreditRateService); private readonly api=inject(CreditSimulationService); private readonly router=inject(Router); private readonly currency=new Intl.NumberFormat('es-EC',{style:'currency',currency:'USD'}); private readonly percentFormat=new Intl.NumberFormat('es-EC',{minimumFractionDigits:2,maximumFractionDigits:2});
  readonly creditTypes=signal<CreditType[]>([]); readonly selectedType=signal<CreditType|null>(null); readonly currentRate=signal<number|null>(null); readonly rateChecked=signal(false); readonly loadingTypes=signal(true); readonly simulating=signal(false); readonly downloadingPdf=signal(false); readonly result=signal<CreditSimulationResponse|null>(null); readonly resultStale=signal(false); readonly showTable=signal(false); readonly showBreakdown=signal(false); readonly error=signal(''); readonly expandedInstallment=signal<number|null>(null);
  readonly minimumStartDate=new Date().toISOString().slice(0,10);
  isPublic():boolean{return this.router.url.startsWith('/simulators/') || this.router.url.startsWith('/client/');}
  readonly form=this.fb.group({creditTypeId:['',Validators.required],amount:[0,Validators.required],termMonths:[0,Validators.required],amortizationSystem:['French' as AmortizationSystem,Validators.required],startDate:[this.minimumStartDate,Validators.required]});
  constructor(){this.typeApi.getAll().pipe(takeUntilDestroyed(this.destroy)).subscribe({next:types=>{this.creditTypes.set(types.filter(x=>x.isActive).map(x=>({...x,minimumAmount:Number(x.minimumAmount),maximumAmount:Number(x.maximumAmount),minimumTermMonths:Number(x.minimumTermMonths),maximumTermMonths:Number(x.maximumTermMonths)})));this.loadingTypes.set(false);this.configureType(this.form.controls.creditTypeId.value);},error:()=>{this.error.set('No se pudo conectar con el servidor.');this.loadingTypes.set(false);}});this.form.controls.creditTypeId.valueChanges.pipe(takeUntilDestroyed(this.destroy)).subscribe(id=>this.configureType(id));this.form.valueChanges.pipe(takeUntilDestroyed(this.destroy)).subscribe(()=>{if(this.result())this.resultStale.set(true);});}
  private configureType(id:string){const type=this.creditTypes().find(x=>x.id===id)??null;this.selectedType.set(type);this.currentRate.set(null);this.rateChecked.set(false);this.form.controls.amount.setValidators(type?[Validators.required,Validators.min(type.minimumAmount),Validators.max(type.maximumAmount)]:[Validators.required,Validators.min(.01)]);this.form.controls.termMonths.setValidators(type?[Validators.required,Validators.min(type.minimumTermMonths),Validators.max(type.maximumTermMonths)]:[Validators.required,Validators.min(1)]);this.form.controls.amount.updateValueAndValidity({emitEvent:false});this.form.controls.termMonths.updateValueAndValidity({emitEvent:false});if(type)this.rateApi.getCurrentRate(type.id).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:rate=>{this.currentRate.set(Number(rate.annualInterestRate));this.rateChecked.set(true);},error:()=>this.rateChecked.set(true)});}
  isValid():boolean{const t=this.selectedType(),v=this.form.getRawValue();return !!t&&this.form.valid&&this.isStartDateValid()&&this.rateChecked()&&this.currentRate()!==null&&v.amount>=t.minimumAmount&&v.amount<=t.maximumAmount&&Number.isInteger(v.termMonths)&&v.termMonths>=t.minimumTermMonths&&v.termMonths<=t.maximumTermMonths;}
  isStartDateValid():boolean{return this.form.controls.startDate.value>=this.minimumStartDate;}
  amountError(){const t=this.selectedType(),v=this.form.controls.amount.value;if(!t||v<=0)return 'Selecciona un tipo e ingresa un monto positivo.';return v<t.minimumAmount||v>t.maximumAmount?`El monto permitido para ${t.name} es de ${this.money(t.minimumAmount)} a ${this.money(t.maximumAmount)}.`:'';}
  termError(){const t=this.selectedType(),v=this.form.controls.termMonths.value;if(!t||!Number.isInteger(v)||v<=0)return 'Selecciona un tipo e ingresa un plazo entero positivo.';return v<t.minimumTermMonths||v>t.maximumTermMonths?`El plazo permitido para ${t.name} es de ${t.minimumTermMonths} a ${t.maximumTermMonths} meses.`:'';}
  selectSystem(s:AmortizationSystem){this.form.controls.amortizationSystem.setValue(s);} money(x:number){return this.currency.format(x);} percent(x:number){return this.percentFormat.format(x)+' %';} systemLabel(s:AmortizationSystem){return s==='French'?'Sistema Francés':'Sistema Alemán';} systemDescription(s:AmortizationSystem){return s==='French'?'La cuota financiera se mantiene prácticamente constante.':'El abono a capital es constante y las cuotas disminuyen progresivamente.';} toggleCharges(i:AmortizationInstallment){this.expandedInstallment.update(x=>x===i.installmentNumber?null:i.installmentNumber);}
  chargeLabel(name:string):string{return name.trim().toLowerCase()==='installment protection'?'Protección de cuotas':name;}
  chargeTotals(simulation:CreditSimulationResponse): [string, number][] {
    const totals = simulation.installments.flatMap(item => item.charges).reduce((result, charge) => {
      result[charge.name] = (result[charge.name] ?? 0) + charge.calculatedAmount;
      return result;
    }, {} as Record<string, number>);
    return Object.entries(totals);
  }
  viewTable(){this.showTable.set(true);setTimeout(()=>document.getElementById('amortization-table')?.scrollIntoView({behavior:'smooth',block:'start'}));}
  simulate(){if(!this.isValid()||this.simulating()){this.form.markAllAsTouched();return;}this.simulating.set(true);this.error.set('');const v=this.form.getRawValue();this.api.simulate({...v,startDate:v.startDate+'T00:00:00Z'} as CreditSimulationRequest).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:r=>{this.result.set(r);this.resultStale.set(false);this.showTable.set(false);this.showBreakdown.set(false);this.simulating.set(false);},error:e=>{this.error.set(this.errorMessage(e));this.simulating.set(false);}});}
  downloadPdf(){if(!this.result()||this.resultStale())return;this.downloadingPdf.set(true);const v=this.form.getRawValue();this.api.downloadPdf({...v,startDate:v.startDate+'T00:00:00Z'} as CreditSimulationRequest).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:b=>{const u=URL.createObjectURL(b),a=document.createElement('a');a.href=u;a.download='simulacion-credito.pdf';a.click();URL.revokeObjectURL(u);this.downloadingPdf.set(false);},error:()=>{this.error.set('No fue posible generar el PDF.');this.downloadingPdf.set(false);}});}
  newSimulation(){this.result.set(null);this.resultStale.set(false);} private errorMessage(e:HttpErrorResponse){return e.status===404?'No existe una tasa vigente para el tipo de crédito seleccionado.':'No fue posible procesar la simulación.';}
}
