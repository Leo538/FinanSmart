export type ChargeType = 'FixedAmount' | 'PercentageOfPrincipal' | 'PercentageOfInstallment';
export type ChargeFrequency = 'OneTime' | 'Monthly';

export interface CreditCharge {
  id: string;
  creditTypeId: string;
  creditTypeName: string;
  name: string;
  description: string | null;
  chargeType: ChargeType;
  value: number;
  frequency: ChargeFrequency;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreditChargeFormData {
  creditTypeId: string;
  name: string;
  description: string | null;
  chargeType: ChargeType;
  value: number;
  frequency: ChargeFrequency;
  isActive: boolean;
}
