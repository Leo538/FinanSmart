import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreditRate, CreditRateFormData } from '../models/credit-rate.model';

@Injectable({ providedIn: 'root' })
export class CreditRateService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.apiUrl + '/api/credit-rates';
  getAll(): Observable<CreditRate[]> { return this.http.get<CreditRate[]>(this.endpoint); }
  getById(id: string): Observable<CreditRate> { return this.http.get<CreditRate>(`${this.endpoint}/${id}`); }
  getByCreditType(creditTypeId: string): Observable<CreditRate[]> { return this.http.get<CreditRate[]>(`${this.endpoint}/credit-type/${creditTypeId}`); }
  getCurrentRate(creditTypeId: string): Observable<CreditRate> { return this.http.get<CreditRate>(`${this.endpoint}/credit-type/${creditTypeId}/current`); }
  create(data: CreditRateFormData): Observable<void> { return this.http.post<void>(this.endpoint, data); }
  update(id: string, data: CreditRateFormData): Observable<void> { return this.http.put<void>(`${this.endpoint}/${id}`, data); }
  delete(id: string): Observable<void> { return this.http.delete<void>(`${this.endpoint}/${id}`); }
}
