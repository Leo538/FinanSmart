import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { InvestmentApplicationDocument, InvestmentDocumentRequirements, InvestmentDocumentType } from '../models/investment-application-document.model';

@Injectable({ providedIn: 'root' })
export class InvestmentApplicationDocumentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/investment-applications`;
  getAll(applicationId: string) { return this.http.get<InvestmentApplicationDocument[]>(`${this.baseUrl}/${applicationId}/documents`); }
  getRequirements(applicationId: string) { return this.http.get<InvestmentDocumentRequirements>(`${this.baseUrl}/${applicationId}/documents/requirements`); }
  upload(applicationId: string, documentType: InvestmentDocumentType, file: File) { const data = new FormData(); data.append('documentType', documentType); data.append('file', file); return this.http.post<InvestmentApplicationDocument>(`${this.baseUrl}/${applicationId}/documents`, data); }
  download(applicationId: string, documentId: string) { return this.http.get(`${this.baseUrl}/${applicationId}/documents/${documentId}/download`, { responseType: 'blob' }); }
  delete(applicationId: string, documentId: string) { return this.http.delete<void>(`${this.baseUrl}/${applicationId}/documents/${documentId}`); }
  complete(applicationId: string) { return this.http.post<void>(`${this.baseUrl}/${applicationId}/documents/complete`, {}); }
}
