export interface AdminDashboardResponse {
  totalApplications: number;
  submittedCount: number;
  docsPendingCount: number;
  underReviewCount: number;
  approvedCount: number;
  rejectedCount: number;
}

export interface AdminApplicationSummary {
  id: string;
  applicationNumber: string;
  applicantUserId: string;
  applicantName: string;
  email: string;
  phone: string;
  requestedAmount: number;
  requestedTenureMonths: number;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  submittedAtUtc: string | null;
}

export interface AdminApplicationStatusHistory {
  status: string;
  remarks: string;
  changedByUserId: string;
  createdAtUtc: string;
}

export interface AdminApplicationDetail {
  id: string;
  applicationNumber: string;
  applicantUserId: string;
  status: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string | null;
  gender: string;
  email: string;
  phone: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  postalCode: string;
  employerName: string;
  employmentType: string;
  monthlyIncome: number | null;
  annualIncome: number | null;
  existingEmiAmount: number;
  requestedAmount: number;
  requestedTenureMonths: number;
  loanPurpose: string;
  remarks: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  submittedAtUtc: string | null;
  timeline: AdminApplicationStatusHistory[];
}

export interface ReviewLoanApplicationRequest {
  targetStatus: string;
  remarks: string;
  interestRate?: number | null;
  sanctionAmount?: number | null;
}
