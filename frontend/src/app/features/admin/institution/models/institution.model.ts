export interface Institution {
  id: string;
  name: string;
  ruc: string;
  email: string;
  phone: string;
  address: string;
  logoUrl: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  backgroundColor: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface InstitutionFormData {
  name: string;
  ruc: string;
  email: string;
  phone: string;
  address: string;
  logoUrl: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  backgroundColor: string | null;
  isActive: boolean;
}
