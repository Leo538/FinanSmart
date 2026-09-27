import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreditCharge, CreditChargeFormData } from '../models/credit-charge.model';

@Injectable({ providedIn: 'root' })
export class CreditChargeService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.apiUrl + '/api/credit-charges';
  getAll(): Observable<CreditCharge[]> { return this.http.get<CreditCharge[]>(this.endpoint); }
  getById(id: string): Observable<CreditCharge> { return this.http.get<CreditCharge>(`${this.endpoint}/${id}`); }
  getByCreditType(creditTypeId: string): Observable<CreditCharge[]> { return this.http.get<CreditCharge[]>(`${this.endpoint}/credit-type/${creditTypeId}`); }
  create(data: CreditChargeFormData): Observable<void> { return this.http.post<void>(this.endpoint, data); }
  update(id: string, data: CreditChargeFormData): Observable<void> { return this.http.put<void>(`${this.endpoint}/${id}`, data); }
  delete(id: string): Observable<void> { return this.http.delete<void>(`${this.endpoint}/${id}`); }
}
