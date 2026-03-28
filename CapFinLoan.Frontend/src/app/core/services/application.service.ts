import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  LoanApplicationResponse,
  LoanApplicationStatusResponse,
  SaveLoanApplicationRequest
} from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/applications`;

  getMyApplications() {
    return this.http.get<LoanApplicationResponse[]>(`${this.apiUrl}/my`);
  }

  getById(id: string) {
    return this.http.get<LoanApplicationResponse>(`${this.apiUrl}/${id}`);
  }

  createDraft(data: SaveLoanApplicationRequest) {
    return this.http.post<LoanApplicationResponse>(this.apiUrl, data);
  }

  updateDraft(id: string, data: SaveLoanApplicationRequest) {
    return this.http.put<LoanApplicationResponse>(`${this.apiUrl}/${id}`, data);
  }

  submit(id: string) {
    return this.http.post<LoanApplicationResponse>(`${this.apiUrl}/${id}/submit`, {});
  }

  getStatus(id: string) {
    return this.http.get<LoanApplicationStatusResponse>(`${this.apiUrl}/${id}/status`);
  }

  deleteDraft(id: string) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
