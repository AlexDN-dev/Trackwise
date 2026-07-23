export enum CandidatureStatus{
  ToSend = 1,
  Sent = 2,
  GotInterview = 3,
  PositiveResponse = 4,
  NegativeResponse = 5,
}

export enum ContractType {
  CDI = 1,
  CDD = 2,
  Interim = 3,
}

export interface CandidatureResponse {
  id: string;
  title: string;
  status: CandidatureStatus;
  location: string | null;
  urlOffer: string | null;
  description: string | null;
  contractType: ContractType;
  postulationDate: string | null;
  createdAt: string | null;
  entrepriseId: string;
  entrepriseName: string | null;
}

export interface CreateCandidatureRequest {
  title: string;
  location?: string | null;
  urlOffer?: string | null;
  description?: string | null;
  contractType: ContractType;
  postulationDate?: string | null;
  entrepriseId?: string | null;
  entrepriseName?: string | null;
}

export interface UpdateCandidatureRequest {
  title: string;
  location?: string | null;
  urlOffer?: string | null;
  description?: string | null;
  contractType: ContractType;
  status: CandidatureStatus;
  postulationDate?: string | null;
  entrepriseId: string;
}

export interface EntrepriseResponse {
  id: string;
  name: string;
  sector: string | null;
  city: string | null;
  website: string | null;
}
