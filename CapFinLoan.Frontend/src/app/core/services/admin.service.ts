import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  AdminDashboardResponse,
  AdminApplicationSummary,
  AdminApplicationDetail,
  ReviewLoanApplicationRequest
} from '../models/admin.models';
import { UserSummary } from '../models/auth.models';
import { DocumentResponse, VerifyDocumentRequest } from '../models/document.models';
import { WalletSummaryResponse } from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  getQueue(status?: string) {
    const params = status ? `?status=${status}` : '';
    return this.http.get<AdminApplicationSummary[]>(`${this.baseUrl}/admin/applications${params}`);
  }

  getDashboard() {
    return this.http.get<AdminDashboardResponse>(`${this.baseUrl}/admin/applications/dashboard`);
  }

  getWalletSummary() {
    return this.http.get<WalletSummaryResponse>(`${this.baseUrl}/applications/wallet/admin/summary`);
  }

  getApplicationById(id: string) {
    return this.http.get<AdminApplicationDetail>(`${this.baseUrl}/admin/applications/${id}`);
  }

  updateStatus(id: string, request: ReviewLoanApplicationRequest) {
    return this.http.put<AdminApplicationDetail>(`${this.baseUrl}/admin/applications/${id}/status`, request);
  }

  getDocsByApplication(applicationId: string) {
    return this.http.get<DocumentResponse[]>(`${this.baseUrl}/admin/documents/application/${applicationId}`);
  }

  verifyDocument(id: string, request: VerifyDocumentRequest) {
    return this.http.put<DocumentResponse>(`${this.baseUrl}/admin/documents/${id}/verify`, request);
  }

  getAllDocuments(status?: string) {
    const params = status && status !== 'All' ? `?status=${status}` : '';
    return this.http.get<DocumentResponse[]>(`${this.baseUrl}/admin/documents${params}`);
  }

  downloadDocument(id: string) {
    return this.http.get(`${this.baseUrl}/admin/documents/${id}/download`, { responseType: 'blob' });
  }

  getUsers() {
    return this.http.get<UserSummary[]>(`${this.baseUrl}/admin/users`);
  }

  updateUserStatus(id: string, isActive: boolean) {
    return this.http.put<UserSummary>(`${this.baseUrl}/admin/users/${id}/status`, { isActive });
  }
}
