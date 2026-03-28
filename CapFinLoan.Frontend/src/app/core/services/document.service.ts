import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DocumentResponse } from '../models/document.models';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/documents`;

  upload(applicationId: string, documentType: string, file: File) {
    const formData = new FormData();
    formData.append('applicationId', applicationId);
    formData.append('documentType', documentType);
    formData.append('file', file);
    return this.http.post<DocumentResponse>(`${this.apiUrl}/upload`, formData);
  }

  replace(documentId: string, file: File, documentType?: string) {
    const formData = new FormData();
    formData.append('file', file);
    if (documentType) {
      formData.append('documentType', documentType);
    }
    return this.http.put<DocumentResponse>(`${this.apiUrl}/${documentId}`, formData);
  }

  getByApplicationId(applicationId: string) {
    return this.http.get<DocumentResponse[]>(`${this.apiUrl}/application/${applicationId}`);
  }

  getMyDocuments() {
    return this.http.get<DocumentResponse[]>(`${this.apiUrl}/my`);
  }

  getById(id: string) {
    return this.http.get<DocumentResponse>(`${this.apiUrl}/${id}`);
  }
}
