import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { CreateInvestmentApplication, InvestmentApplication, UpdateInvestmentApplicant, UpdateInvestmentDeclarations } from '../models/investment-application.model';

@Injectable({ providedIn: 'root' })
export class InvestmentApplicationService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/investment-applications`;
  create(request: CreateInvestmentApplication) { return this.http.post<InvestmentApplication>(this.url, request); }
  getAll() { return this.http.get<InvestmentApplication[]>(this.url); }
  getMine() { return this.http.get<InvestmentApplication[]>(`${this.url}/mine`); }
  getById(id: string) { return this.http.get<InvestmentApplication>(`${this.url}/${id}`); }
  updateApplicant(id: string, request: UpdateInvestmentApplicant) { return this.http.put<InvestmentApplication>(`${this.url}/${id}/applicant`, request); }
  updateDeclarations(id: string, request: UpdateInvestmentDeclarations) { return this.http.put<InvestmentApplication>(`${this.url}/${id}/declarations`, request); }
  cancel(id: string) { return this.http.delete<void>(`${this.url}/${id}`); }
  submit(id: string) { return this.http.post<InvestmentApplication>(`${this.url}/${id}/submit`, {}); }
  review(id: string, decision: 'Approve' | 'Reject', notes: string | null) { return this.http.post<InvestmentApplication>(`${this.url}/${id}/review`, { decision, notes }); }
}
