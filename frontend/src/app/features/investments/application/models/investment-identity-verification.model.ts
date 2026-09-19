export type IdentityVerificationStatus='Pending'|'Captured'|'Verified'|'Rejected';
export interface InvestmentIdentityVerification { id:string; investmentApplicationId:string; status:IdentityVerificationStatus; consentAccepted:boolean; selfieContentType:string|null; selfieFileSize:number|null; capturedAt:string|null; verifiedAt:string|null; }
