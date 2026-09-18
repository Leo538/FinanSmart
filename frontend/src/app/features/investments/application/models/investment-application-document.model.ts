export type InvestmentDocumentType = 'IdentityFront' | 'IdentityBack' | 'AdditionalDocument';
export interface InvestmentApplicationDocument { id: string; investmentApplicationId: string; documentType: InvestmentDocumentType; originalFileName: string; contentType: string; fileSize: number; uploadedAt: string; isActive: boolean; }
export interface InvestmentDocumentRequirements { requiredTypes: InvestmentDocumentType[]; uploadedTypes: InvestmentDocumentType[]; missingTypes: InvestmentDocumentType[]; isComplete: boolean; }
