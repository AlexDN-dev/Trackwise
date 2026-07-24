import { ContractType } from './candidature.models';

export interface AnalysisResult {
  title: string | null;
  entrepriseName: string | null;
  location: string | null;
  contractType: ContractType | null;
  urlOffer: string | null;
  description: string | null;
  competences: string[];
}

export interface AnalyzeRequest {
  text: string;
}

export type AiJobStatus = 'running' | 'succeeded' | 'failed';

export interface AiJob {
  id: string;
  status: AiJobStatus;
  startedAt: Date;
  result?: AnalysisResult;
  error?: string;
}
