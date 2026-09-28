export interface CreditType {
  id: string;
  name: string;
  description: string | null;
  minimumAmount: number | null;
  maximumAmount: number | null;
  minimumTermMonths: number | null;
  maximumTermMonths: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreditTypeFormData {
  name: string;
  description: string | null;
  minimumAmount: number | null;
  maximumAmount: number | null;
  minimumTermMonths: number | null;
  maximumTermMonths: number | null;
  isActive: boolean;
}
