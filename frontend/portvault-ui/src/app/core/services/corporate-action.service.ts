import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
import {
  CorporateActionResponse,
  CreateCorporateActionRequest,
  UpdateCorporateActionRequest,
} from '../../models/corporate-action.model';

@Injectable({
  providedIn: 'root',
})
export class CorporateActionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/CorporateAction`;

  getAll(): Observable<ApiResponse<CorporateActionResponse[]>> {
    return this.http.get<ApiResponse<CorporateActionResponse[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<CorporateActionResponse>> {
    return this.http.get<ApiResponse<CorporateActionResponse>>(`${this.apiUrl}/${id}`);
  }

  getByInstrumentId(instrumentId: number): Observable<ApiResponse<CorporateActionResponse[]>> {
    return this.http.get<ApiResponse<CorporateActionResponse[]>>(
      `${this.apiUrl}/instrument/${instrumentId}`
    );
  }

  create(request: CreateCorporateActionRequest): Observable<ApiResponse<CorporateActionResponse>> {
    return this.http.post<ApiResponse<CorporateActionResponse>>(this.apiUrl, request);
  }

  update(
    id: number,
    request: UpdateCorporateActionRequest
  ): Observable<ApiResponse<CorporateActionResponse>> {
    return this.http.put<ApiResponse<CorporateActionResponse>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl}/${id}`);
  }
}
