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

export interface WalletSummaryResponse {
  walletAccountId: string;
  ownerUserId: string;
  ownerType: string;
  currency: string;
  balance: number;
  recentEntries: WalletLedgerEntryResponse[];
}

export interface WalletLedgerEntryResponse {
  id: string;
  walletAccountId: string;
  direction: string;
  entryType: string;
  amount: number;
  currency: string;
  status: string;
  referenceId: string | null;
  correlationId: string | null;
  remarks: string | null;
  createdAtUtc: string;
}

export interface CreateTopUpOrderRequest {
  amount: number;
  currency?: string | null;
}

export interface CreateTopUpOrderResponse {
  provider: string;
  keyId: string;
  providerOrderId: string;
  amount: number;
  currency: string;
  referenceId: string;
}

export interface VerifyTopUpRequest {
  providerOrderId: string;
  providerPaymentId: string;
  providerSignature: string;
}

export interface VerifyTopUpResponse {
  success: boolean;
  message: string;
  wallet: WalletSummaryResponse;
}

export interface WalletConfigResponse {
  applicationFee: number;
  minTopUpAmount: number;
  maxTopUpAmount: number;
  currency: string;
}
