import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreditType, CreditTypeFormData } from '../models/credit-type.model';

@Injectable({ providedIn: 'root' })
export class CreditTypeService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.apiUrl + '/api/credit-types';

  getAll(): Observable<CreditType[]> {
    return this.http.get<CreditType[]>(this.endpoint);
  }

  getById(id: string): Observable<CreditType> {
    return this.http.get<CreditType>(`${this.endpoint}/${id}`);
  }

  create(data: CreditTypeFormData): Observable<void> {
    return this.http.post<void>(this.endpoint, data);
  }

  update(id: string, data: CreditTypeFormData): Observable<void> {
    return this.http.put<void>(`${this.endpoint}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}
