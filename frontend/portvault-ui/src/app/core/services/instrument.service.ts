import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
import {
  InstrumentResponse,
  CreateInstrumentRequest,
  UpdateInstrumentRequest,
  AddInstrumentIdentifierRequest,
  InstrumentIdentifierResponse,
  InstrumentDependenciesResponse,
  MigrateInstrumentRequest,
  InstrumentMigrationResponse,
} from '../../models/instrument.model';

@Injectable({
  providedIn: 'root',
})
export class InstrumentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Instrument`;

  getAll(search?: string): Observable<ApiResponse<InstrumentResponse[]>> {
    if (search) {
      return this.http.get<ApiResponse<InstrumentResponse[]>>(this.apiUrl, { params: { search } });
    }
    return this.http.get<ApiResponse<InstrumentResponse[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<InstrumentResponse>> {
    return this.http.get<ApiResponse<InstrumentResponse>>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateInstrumentRequest): Observable<ApiResponse<InstrumentResponse>> {
    return this.http.post<ApiResponse<InstrumentResponse>>(this.apiUrl, request);
  }

  update(
    id: number,
    request: UpdateInstrumentRequest
  ): Observable<ApiResponse<InstrumentResponse>> {
    return this.http.put<ApiResponse<InstrumentResponse>>(`${this.apiUrl}/${id}`, request);
  }

  addIdentifier(
    instrumentId: number,
    identifier: AddInstrumentIdentifierRequest
  ): Observable<ApiResponse<InstrumentIdentifierResponse>> {
    return this.http.post<ApiResponse<InstrumentIdentifierResponse>>(
      `${this.apiUrl}/${instrumentId}/identifiers`,
      identifier
    );
  }

  moveIdentifier(
    instrumentId: number,
    identifierId: number
  ): Observable<ApiResponse<InstrumentIdentifierResponse>> {
    return this.http.patch<ApiResponse<InstrumentIdentifierResponse>>(
      `${this.apiUrl}/${instrumentId}/identifiers/${identifierId}/move`,
      {}
    );
  }

  deleteIdentifier(identifierId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl}/identifiers/${identifierId}`);
  }

  getDependencies(id: number): Observable<ApiResponse<InstrumentDependenciesResponse>> {
    return this.http.get<ApiResponse<InstrumentDependenciesResponse>>(
      `${this.apiUrl}/${id}/dependencies`
    );
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl}/${id}`);
  }

  migrate(
    sourceId: number,
    request: MigrateInstrumentRequest
  ): Observable<ApiResponse<InstrumentMigrationResponse>> {
    return this.http.post<ApiResponse<InstrumentMigrationResponse>>(
      `${this.apiUrl}/${sourceId}/migrate`,
      request
    );
  }
}
