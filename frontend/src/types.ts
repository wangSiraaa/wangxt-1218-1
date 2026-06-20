export interface AuthUser {
  id: number
  username: string
  fullName: string
  role: string
}

export interface LoginResponse {
  token: string
  user: AuthUser
}

export interface CaseDto {
  id: number
  caseNumber: string
  title: string
  stage: string
  createdBy: number
  creatorName: string
  createdAt: string
  evidenceCount: number
}

export interface EvidenceDto {
  id: number
  caseId: number
  name: string
  sha256: string
  uploadedHash: string
  status: string
  isAdopted: boolean
  hashVerified: boolean
  uploadedBy: number
  uploaderName: string
  uploadedAt: string
}

export interface AdoptionDto {
  id: number
  evidenceId: number
  reviewerId: number
  reviewerName: string
  opinion: string
  adopted: boolean
  createdAt: string
}

export interface RetrievalLogDto {
  id: number
  evidenceId: number
  evidenceName: string
  caseId: number
  caseNumber: string
  userId: number
  userName: string
  purpose: string
  retrievedAt: string
}

export interface EvidenceDetailDto extends EvidenceDto {
  filePath: string
  adoptions: AdoptionDto[]
  retrievals: RetrievalLogDto[]
}

export interface VerifyResult {
  evidenceId: number
  integrityOk: boolean
}
