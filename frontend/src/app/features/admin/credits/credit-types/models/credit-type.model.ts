export interface CreditType {
  id: string;
  name: string;
  description: string | null;
  minimumAmount: number;
  maximumAmount: number;
  minimumTermMonths: number;
  maximumTermMonths: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreditTypeFormData {
  name: string;
  description: string | null;
  minimumAmount: number;
  maximumAmount: number;
  minimumTermMonths: number;
  maximumTermMonths: number;
  isActive: boolean;
}
