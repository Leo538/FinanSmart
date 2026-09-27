export interface CreditRate {
  id: string;
  creditTypeId: string;
  creditTypeName: string;
  annualInterestRate: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreditRateFormData {
  creditTypeId: string;
  annualInterestRate: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
}
