import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { Institution, InstitutionFormData } from '../models/institution.model';

@Injectable({ providedIn: 'root' })
export class InstitutionService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.apiUrl + '/api/institutions';

  getAll(): Observable<Institution[]> { return this.http.get<Institution[]>(this.endpoint); }
  getById(id: string): Observable<Institution> { return this.http.get<Institution>(this.endpoint + '/' + id); }
  create(data: InstitutionFormData): Observable<Institution> { return this.http.post<Institution>(this.endpoint, data); }
  update(id: string, data: InstitutionFormData): Observable<void> { return this.http.put<void>(this.endpoint + '/' + id, data); }
}
