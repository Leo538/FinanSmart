import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { CreditComparisonRequest, CreditComparisonResponse } from '../models/credit-comparison.model';
@Injectable({ providedIn: 'root' })
export class CreditComparisonService {
  private readonly http = inject(HttpClient);
  compare(request: CreditComparisonRequest): Observable<CreditComparisonResponse> {
    return this.http.post<CreditComparisonResponse>(environment.apiUrl + '/api/credit-comparisons/compare', request);
  }
}
