import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  ApplicantProfileResponse,
  CreateTopUpOrderRequest,
  CreateTopUpOrderResponse,
  LoanApplicationResponse,
  LoanApplicationStatusResponse,
  SaveApplicantProfileRequest,
  SaveLoanApplicationRequest,
  VerifyTopUpRequest,
  VerifyTopUpResponse,
  WalletConfigResponse,
  WalletLedgerEntryResponse,
  WalletSummaryResponse,
  WithdrawRequest
} from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/applications`;

  getMyApplications() {
    return this.http.get<LoanApplicationResponse[]>(`${this.apiUrl}/my`);
  }

  getProfile() {
    return this.http.get<ApplicantProfileResponse>(`${this.apiUrl}/profile`);
  }

  saveProfile(data: SaveApplicantProfileRequest) {
    return this.http.put<ApplicantProfileResponse>(`${this.apiUrl}/profile`, data);
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

  getWalletSummary() {
    return this.http.get<WalletSummaryResponse>(`${this.apiUrl}/wallet/summary`);
  }

  getWalletLedger(take = 20) {
    return this.http.get<WalletLedgerEntryResponse[]>(`${this.apiUrl}/wallet/ledger?take=${take}`);
  }

  getWalletConfig() {
    return this.http.get<WalletConfigResponse>(`${this.apiUrl}/wallet/config`);
  }

  createTopUpOrder(data: CreateTopUpOrderRequest) {
    return this.http.post<CreateTopUpOrderResponse>(`${this.apiUrl}/wallet/topup/create-order`, data);
  }

  verifyTopUp(data: VerifyTopUpRequest) {
    return this.http.post<VerifyTopUpResponse>(`${this.apiUrl}/wallet/topup/verify`, data);
  }

  withdraw(data: WithdrawRequest) {
    return this.http.post<WalletSummaryResponse>(`${this.apiUrl}/wallet/withdraw`, data);
  }

  getStatus(id: string) {
    return this.http.get<LoanApplicationStatusResponse>(`${this.apiUrl}/${id}/status`);
  }

  deleteDraft(id: string) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getAdminWalletSummary() {
    return this.http.get<WalletSummaryResponse>(`${this.apiUrl}/wallet/admin/summary`);
  }

  createAdminTopUpOrder(data: CreateTopUpOrderRequest) {
    return this.http.post<CreateTopUpOrderResponse>(`${this.apiUrl}/wallet/admin/topup/create-order`, data);
  }

  verifyAdminTopUp(data: VerifyTopUpRequest) {
    return this.http.post<VerifyTopUpResponse>(`${this.apiUrl}/wallet/admin/topup/verify`, data);
  }
}
