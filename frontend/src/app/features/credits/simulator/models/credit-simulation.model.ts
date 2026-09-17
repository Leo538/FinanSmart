export type AmortizationSystem = 'French' | 'German';
export interface CreditSimulationRequest { creditTypeId: string; amount: number; termMonths: number; amortizationSystem: AmortizationSystem; startDate: string; }
export interface AppliedCharge { creditChargeId: string; name: string; chargeType: string; frequency: string; configuredValue: number; calculatedAmount: number; }
export interface AmortizationInstallment { installmentNumber: number; dueDate: string; openingBalance: number; principalPayment: number; interestPayment: number; basePayment: number; additionalCharges: number; totalPayment: number; closingBalance: number; charges: AppliedCharge[]; }
export interface CreditSimulationResponse {
  creditTypeId: string; creditTypeName: string; requestedAmount: number; termMonths: number; amortizationSystem: AmortizationSystem;
  annualInterestRate: number; monthlyInterestRate: number; startDate: string; baseFirstPayment: number; baseLastPayment: number;
  totalPrincipal: number; totalInterest: number; totalCharges: number; totalPayment: number; generatedAt: string; installments: AmortizationInstallment[];
}
