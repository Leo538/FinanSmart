import { AmortizationSystem } from '../../simulator/models/credit-simulation.model';
export interface CreditComparisonRequest { creditTypeId: string; amount: number; termMonths: number; startDate: string; }
export interface AmortizationComparisonSummary { amortizationSystem: AmortizationSystem; firstPayment: number; lastPayment: number; totalPrincipal: number; totalInterest: number; totalCharges: number; totalPayment: number; }
export interface CreditComparisonResponse {
  creditTypeId: string; creditTypeName: string; requestedAmount: number; termMonths: number; annualInterestRate: number; startDate: string; generatedAt: string;
  french: AmortizationComparisonSummary; german: AmortizationComparisonSummary;
  interestDifference: number; chargesDifference: number; totalPaymentDifference: number; firstPaymentDifference: number; lastPaymentDifference: number;
}
