import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { Institution, InstitutionFormData } from '../models/institution.model';

@Injectable({ providedIn: 'root' })
export class InstitutionService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.apiUrl + '/api/institutions';

  getAll(): Observable<Institution[]> { return this.http.get<Institution[]>(this.endpoint).pipe(map(items => items.map(item => this.withLogoUrl(item)))); }
  getById(id: string): Observable<Institution> { return this.http.get<Institution>(this.endpoint + '/' + id).pipe(map(item => this.withLogoUrl(item))); }
  create(data: InstitutionFormData): Observable<Institution> { return this.http.post<Institution>(this.endpoint, data); }
  update(id: string, data: InstitutionFormData): Observable<void> { return this.http.put<void>(this.endpoint + '/' + id, data); }
  uploadLogo(id: string, file: File): Observable<Institution> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<Institution>(`${this.endpoint}/${id}/logo`, formData).pipe(map(item => this.withLogoUrl(item)));
  }
  deleteLogo(id: string): Observable<void> { return this.http.delete<void>(`${this.endpoint}/${id}/logo`); }
  private withLogoUrl(institution: Institution): Institution {
    return institution.logoUrl?.startsWith('/storage/') ? { ...institution, logoUrl: environment.apiUrl + institution.logoUrl } : institution;
  }
}
