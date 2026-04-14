export interface PersonalDetailsRequest {
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
}

export interface EmploymentDetailsRequest {
  employerName: string;
  employmentType: string;
  monthlyIncome: number | null;
  annualIncome: number | null;
  existingEmiAmount: number;
}

export interface LoanDetailsRequest {
  requestedAmount: number;
  requestedTenureMonths: number;
  loanPurpose: string;
  remarks: string;
}

export interface SaveLoanApplicationRequest {
  personalDetails: PersonalDetailsRequest;
  employmentDetails: EmploymentDetailsRequest;
  loanDetails: LoanDetailsRequest;
}

export interface SaveApplicantProfileRequest {
  personalDetails: PersonalDetailsRequest;
  employmentDetails: EmploymentDetailsRequest;
}

export interface PersonalDetailsResponse {
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
}

export interface EmploymentDetailsResponse {
  employerName: string;
  employmentType: string;
  monthlyIncome: number | null;
  annualIncome: number | null;
  existingEmiAmount: number;
}

export interface LoanDetailsResponse {
  requestedAmount: number;
  requestedTenureMonths: number;
  loanPurpose: string;
  remarks: string;
}

export interface LoanApplicationResponse {
  id: string;
  applicationNumber: string;
  applicantUserId: string;
  status: string;
  personalDetails: PersonalDetailsResponse;
  employmentDetails: EmploymentDetailsResponse;
  loanDetails: LoanDetailsResponse;
  createdAtUtc: string;
  updatedAtUtc: string;
  submittedAtUtc: string | null;
}

export interface ApplicantProfileResponse {
  applicantUserId: string;
  personalDetails: PersonalDetailsResponse;
  employmentDetails: EmploymentDetailsResponse;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ApplicationStatusHistoryResponse {
  status: string;
  remarks: string;
  changedByUserId: string;
  createdAtUtc: string;
}

export interface LoanApplicationStatusResponse {
  id: string;
  applicationNumber: string;
  currentStatus: string;
  timeline: ApplicationStatusHistoryResponse[];
}
