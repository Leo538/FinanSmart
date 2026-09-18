export type InvestmentApplicationStatus = 'Draft' | 'PendingDocuments' | 'PendingIdentityVerification' | 'ReadyForReview' | 'Submitted' | 'Approved' | 'Rejected' | 'Cancelled';
export type InvestmentApplicationStep = 'Investment' | 'PersonalInformation' | 'Documents' | 'IdentityVerification' | 'Review' | 'Confirmation';
export type IdentificationType = 'NationalId' | 'Passport';
export interface CreateInvestmentApplication { investmentProductId: string; amount: number; termDays: number; startDate: string; }
export interface UpdateInvestmentApplicant { firstName: string; lastName: string; identificationType: IdentificationType; identificationNumber: string; email: string; phone: string; birthDate: string | null; address: string; city: string; }
export interface InvestmentApplication {
  id: string; applicationNumber: string; investmentProductId: string; investmentProductName: string; amount: number; termDays: number;
  annualInterestRate: number; interestCalculationMethod: string; interestPaymentFrequency: string; startDate: string; maturityDate: string;
  totalInterest: number; principalAtMaturity: number; totalReceived: number; status: InvestmentApplicationStatus; currentStep: InvestmentApplicationStep;
  applicantFirstName: string | null; applicantLastName: string | null; identificationType: IdentificationType | null; identificationNumber: string | null;
  email: string | null; phone: string | null; birthDate: string | null; address: string | null; city: string | null;
  createdAt: string; updatedAt: string; submittedAt: string | null;
}
