import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { CreditSimulationRequest, CreditSimulationResponse } from '../models/credit-simulation.model';
@Injectable({ providedIn: 'root' })
export class CreditSimulationService {
  private readonly http = inject(HttpClient);
  simulate(request: CreditSimulationRequest): Observable<CreditSimulationResponse> {
    return this.http.post<CreditSimulationResponse>(environment.apiUrl + '/api/credit-simulations/simulate', request);
  }

  downloadPdf(request: CreditSimulationRequest): Observable<Blob> {
    return this.http.post(environment.apiUrl + '/api/credit-simulations/pdf', request, { responseType: 'blob' });
  }
}
