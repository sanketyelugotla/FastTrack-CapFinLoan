export type DocumentStatus = 'Pending' | 'UnderReview' | 'Verified' | 'ReuploadRequired';

export interface DocumentResponse {
  id: string;
  applicationId: string;
  userId: string;
  documentType: string;
  fileName: string;
  filePath: string;
  contentType: string;
  fileSizeBytes: number;
  /** Explicit status from backend: Pending | UnderReview | Verified | ReuploadRequired */
  status: DocumentStatus;
  // Legacy fields — still returned by backend for compatibility
  isVerified: boolean;
  remarks: string;
  verifiedByUserId: string | null;
  verifiedAtUtc: string | null;
  createdAtUtc: string;
  uploadedAtUtc?: string;
  updatedAtUtc: string;
}

export interface VerifyDocumentRequest {
  isVerified: boolean;
  remarks: string;
}
